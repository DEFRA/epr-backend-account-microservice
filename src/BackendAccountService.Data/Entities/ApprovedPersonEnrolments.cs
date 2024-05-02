using System;
using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Data.Entities
{
    public class ApprovedPersonEnrolment : ISoftDeletableEntity, IEditableEntity
    {
        public int Id { get; set; }
        
        public int EnrolmentId { get; set; }

        public Enrolment Enrolment { get; set; }

        [MaxLength(450)]
        public string? NomineeDeclaration { get; set; }

        public DateTimeOffset NomineeDeclarationTime { get; set; }

        public DateTimeOffset CreatedOn { get; private set; }

        public DateTimeOffset LastUpdatedOn { get; private set; }

        public bool IsDeleted { get; set; }
    }
}