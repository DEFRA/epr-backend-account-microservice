CREATE OR ALTER PROCEDURE [dbo].[GenerateSubsidiaryData] (
	 @RowNumber INT
	,@Status VARCHAR(50) OUTPUT
	,@Error VARCHAR(MAX) OUTPUT
	,@Info VARCHAR(MAX) OUTPUT
)
AS
BEGIN

    SET NOCOUNT ON

	DECLARE @Counter INT = 1;
	DECLARE @TotalRows INT;
	DECLARE @Organisations_identity INT;
	DECLARE @current_datetime DATETIME2;
	DECLARE @organisation_id VARCHAR(100);
	DECLARE @org_pk_id VARCHAR(100);
	DECLARE @subsidiary_id VARCHAR(100);
	DECLARE @organisation_type_code VARCHAR(100);
	DECLARE @companies_house_number VARCHAR(100);
	DECLARE @organisation_name VARCHAR(100);
	DECLARE @trading_name VARCHAR(100);
	DECLARE @registered_addr_line1 VARCHAR(100);
	DECLARE @registered_addr_line2 VARCHAR(100);
	DECLARE @registered_city VARCHAR(100);
	DECLARE @registered_addr_county VARCHAR(100);
	DECLARE @registered_addr_postcode VARCHAR(100);
	DECLARE @registered_addr_country VARCHAR(100);
	DECLARE @home_nation_code VARCHAR(100);
	DECLARE @systemUserEmail NVARCHAR(254) = 'system@dummy.com'
	DECLARE @systemUserId INT
	DECLARE @systemOrganisationId INT
 
	DECLARE @InfoMessage VARCHAR(MAX) = '';
	SET @current_datetime = current_timestamp ;

	SELECT @organisation_id = organisation_id,
		@org_pk_id = org_pk_id,
		@subsidiary_id = subsidiary_id,
		@organisation_type_code = organisation_type_code,
		@companies_house_number = companies_house_number,
		@organisation_name = organisation_name,
		@trading_name = trading_name,
		@registered_addr_line1 = registered_addr_line1,
		@registered_addr_line2 = registered_addr_line2,
		@registered_city = registered_city,
		@registered_addr_county = registered_addr_county,
		@registered_addr_postcode = registered_addr_postcode,
		@registered_addr_country = registered_addr_country,
		@home_nation_code = home_nation_code		
	FROM dbo.SubsidiaryExtract 
	WHERE RowNumber = @RowNumber;

	SET @InfoMessage = @InfoMessage + 'RowNumber : ' + TRIM(STR(@RowNumber)) + ' | ';
	SET @InfoMessage = @InfoMessage + 'organisation_id : ' + ISNULL(@organisation_id,'') + ' | ';
    SET @InfoMessage = @InfoMessage + 'subsidiary_id : ' + ISNULL(@subsidiary_id,'') + ' | ';
	SET @InfoMessage = @InfoMessage + 'companies_house_number : ' + ISNULL(@companies_house_number,'') + ' | ';

	IF  NOT EXISTS	(SELECT 1
						FROM [dbo].[OrganisationRelationships] Rel
						INNER JOIN [dbo].[SubsidiaryOrganisations] SO ON SO.OrganisationId = Rel.SecondOrganisationId
						INNER JOIN [dbo].[Organisations] org ON org.Id = SO.OrganisationId
						WHERE Rel.RelationToDate IS NULL
						AND SO.SubsidiaryId = @subsidiary_id 
						AND Rel.FirstOrganisationId = @org_pk_id 
						AND ISNULL(org.CompaniesHouseNumber,'') = ISNULL(@companies_house_number,''))
	BEGIN
		SET @InfoMessage = @InfoMessage + 'This record does not exist in the system hence it will be processed now.' + ' | ';
        SET @InfoMessage = @InfoMessage + 'organisation_name : ' + ISNULL(@organisation_name,'') + ' | ';
        SET @InfoMessage = @InfoMessage + 'trading_name : ' + ISNULL(@trading_name,'') + ' | ';
			
		BEGIN TRY
			BEGIN TRANSACTION;
				SET @InfoMessage = @InfoMessage + 'Started the Transaction.' + ' | ';

				SET @InfoMessage = @InfoMessage + 'Fetching System user id and system organisation id.' + ' | ';
	
	            SELECT @systemUserId = u.[Id]
	            	  ,@systemOrganisationId = o.[Id]
	            FROM [dbo].[Users] u
	            INNER JOIN [dbo].[Persons] p
	            on p.[UserId] = u.[Id]
	            LEFT  JOIN [dbo].[PersonOrganisationConnections] poc
	            on poc.[PersonId] = p.[Id]
	            LEFT  JOIN [dbo].[Organisations] o
	            on o.[Id] = poc.[OrganisationId]
	            where u.[Email] = @systemUserEmail
 	            
	            IF @systemUserId IS NULL OR @systemOrganisationId IS NULL
	            BEGIN
				    SET @InfoMessage = @InfoMessage + 'System user id or system organisation id was not found.' + ' | ';
	            	THROW 50000, 'System user id or system organisation id was not found', 1;
	            END


                SET @InfoMessage = @InfoMessage + 'Inserting into [dbo].[Organisations]' + ' | ';
				
				INSERT INTO [dbo].[Organisations]
				([OrganisationTypeId]
				,[CompaniesHouseNumber]
				,[Name]
				,[TradingName]
				,[SubBuildingName]
				,[BuildingName]
				,[BuildingNumber]
				,[Street]
				,[Locality]
				,[DependentLocality]
				,[Town]
				,[County]
				,[Country]
				,[Postcode]
				,[ValidatedWithCompaniesHouse]
				,[IsComplianceScheme]
				,[NationId]
				,[CreatedOn]
				,[LastUpdatedOn]
				,[IsDeleted]
				,[ProducerTypeId]
				,[TransferNationId]
				)
				SELECT
				(CASE
				    WHEN @companies_house_number IS NOT NULL THEN 1
					ELSE 2
			    END)
				,@companies_house_number
				,@organisation_name
				,@trading_name
				,NULL
				,NULL
				,NULL
				,@registered_addr_line1
				,@registered_addr_line2
				,NULL
				,@registered_city
				,@registered_addr_county
				,@registered_addr_country
				,@registered_addr_postcode
				,0
				,0
				,(CASE 
					WHEN @home_nation_code = 'EN' THEN 1 
					WHEN @home_nation_code = 'NI' THEN 2 
					WHEN @home_nation_code = 'SC' THEN 3 
					WHEN @home_nation_code = 'WS' THEN 4 
					ELSE NULL
				END) 
				,@current_datetime
				,@current_datetime
				,0
				,NULL
				,NULL;
			   
				SET @Organisations_identity = @@IDENTITY;
				
				SET @InfoMessage = @InfoMessage + 'Organisations_identity : ' + CAST(@Organisations_identity AS VARCHAR) + ' | ';
			    SET @InfoMessage = @InfoMessage + 'Inserting into [dbo].[SubsidiaryOrganisations]' + ' | ';

				INSERT INTO [dbo].[SubsidiaryOrganisations]
				([OrganisationId]
				,[SubsidiaryId]
				,[CreatedOn]
				,[LastUpdatedOn])
				SELECT 
				 @Organisations_identity
				,@subsidiary_id
				,@current_datetime
				,@current_datetime ;
			   
		   		SET @InfoMessage = @InfoMessage + 'Inserting into [dbo].[OrganisationRelationships]' + ' | ';
				

				INSERT INTO [dbo].[OrganisationRelationships]
				([FirstOrganisationId]
				,[SecondOrganisationId]
				,[OrganisationRelationshipTypeId]
				,[RelationFromDate]
				,[RelationToDate]
				,[RelationExpiryReason]
				,[CreatedOn]
				,[LastUpdatedById] 
				,[LastUpdatedOn]
				,[LastUpdatedByOrganisationId]
				,[OrganisationRegistrationTypeId]
				)
				SELECT 
				 @org_pk_id
				,@Organisations_identity
				,1
				,@current_datetime
				,NULL
				,NULL
				,@current_datetime
				,@systemUserId		 
				,@current_datetime
				,@systemOrganisationId
				,NULL ;

			COMMIT TRANSACTION;
			SET @Status = 'Success';
			SET @Info = @InfoMessage + 'Transaction has been successful. Transaction Committed.' + ' | ';
            SET @Error = NULL;
		END TRY
		   
		BEGIN CATCH
			ROLLBACK TRANSACTION;
			SET @Status = 'Failure';
			SET @InfoMessage = ISNULL(@InfoMessage, '') 
			SET @Info = @InfoMessage + 'Transaction has failed. Transaction Rolled back.' + ' | ';
            SET @Error = ERROR_MESSAGE();
		END CATCH
	END
    ELSE
    BEGIN
        SET @Status = 'Success';
        SET @Error = NULL;
		SET @Info = @InfoMessage + 'Record already exists. Hence ignored.' + ' | ';
    END
END
