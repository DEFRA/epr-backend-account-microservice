## Summary
To facilitate the testing of the complete LAPS user-sign journey we need to be able to associated generated users in 
the EPR Accounts database with user accounts created in the Azure AD B2C tenant.

The data-seeder application will look for a text file in /TestUsers containing a list of 6 email address and their B2C oid's to add to the 6 users created in `DataGenerator.GenerateStableLocalAuthorityData`. 

The oid's are taken from pre-existing Azure AD B2C user credentials you have created and can login with access to an email inbox to action the verification step.

## Example

Provide a file using the following format:

/TestUsers/user-ids.txt
```
// 1 user who works for a Welsh LA
me+1@example.com|00000000-0000-0000-0000-000000000000

// 1 user who works for a English LA
me+2@example.com|00000000-0000-0000-0000-000000000000

// 1 user who works for a Scottish LA
me+3@example.com|00000000-0000-0000-0000-000000000000

// 1 user who works for a NI LA
me+4@example.com|00000000-0000-0000-0000-000000000000

// 1 user who works for 2 English LA
me+5@example.com|00000000-0000-0000-0000-000000000000

// 1 user who works for 6 English LA
me+6@example.com|00000000-0000-0000-0000-000000000000
```

> Note: Line comments starting with // will be ignored by the file parser
> The file name can be anything, the program will just load in the most recent file with the .txt extension