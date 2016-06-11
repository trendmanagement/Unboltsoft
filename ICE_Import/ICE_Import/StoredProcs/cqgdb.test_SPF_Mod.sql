CREATE PROCEDURE [cqgdb].[test_SPF_Mod]
    @contractname varchar(45),
    @month char,
    @monthint int,
    @year int,
    @idinstrument int,
    @cqgsymbol varchar(45)
AS

SET NOCOUNT ON;    

MERGE INTO cqgdb.test_tblcontracts as tgt_tblcontracts

USING
    (SELECT * FROM [cqgdb].tblcontractexpirations WHERE optionmonthint = @monthint  AND optionyear = @year AND optionmonthint = @idinstrument)
    AS src_tblcontractexpirations
    ON tgt_tblcontracts.month = @month
    AND tgt_tblcontracts.year = @year
    AND tgt_tblcontracts.idinstrument = @idinstrument
    AND tgt_tblcontracts.expirationdate = src_tblcontractexpirations.expirationdate 

WHEN MATCHED THEN

UPDATE
    SET cqgsymbol = @cqgsymbol
    
WHEN NOT MATCHED THEN

    INSERT (contractname,
            month,
            monthint,
            year,
            idinstrument,
            expirationdate,
            cqgsymbol)
    VALUES (@contractname,
            @month,
            @monthint,
            @year,
            @idinstrument,
            src_tblcontractexpirations.expirationdate,
            @cqgsymbol);

SET NOCOUNT OFF;