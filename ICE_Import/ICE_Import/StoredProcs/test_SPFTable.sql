CREATE PROCEDURE [cqgdb].[test_SPFTable]
    @contract ContractType READONLY
AS

SET NOCOUNT ON;    

MERGE INTO cqgdb.test_tblcontracts as tgt

USING @contract AS src
    ON tgt.month = src.month
    AND tgt.year = src.year
    AND tgt.idinstrument = src.idinstrument

WHEN MATCHED THEN

UPDATE
    SET cqgsymbol = src.cqgsymbol
    
WHEN NOT MATCHED THEN

    INSERT (contractname,
            month,
            monthint,
            year,
            idinstrument,
            expirationdate,
            cqgsymbol)
    VALUES (src.contractname,
            src.month,
            src.monthint,
            src.year,
            src.idinstrument,
            src.expirationdate,
            src.cqgsymbol);

SET NOCOUNT OFF;