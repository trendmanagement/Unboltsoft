CREATE PROCEDURE [cqgdb].[test_SPF]
    @contractname varchar(45),
    @month char,
    @monthint int,
    @year int,
    @idinstrument int,
    @expirationdate date,
    @cqgsymbol varchar(45)
AS

SET NOCOUNT ON;    

MERGE INTO cqgdb.test_tblcontracts as tgt_tblcontracts

USING
    (SELECT @month, @year, @idinstrument)
    AS src_tblcontracts (month, year, idinstrument)
    ON tgt_tblcontracts.month = src_tblcontracts.month
    AND tgt_tblcontracts.year = src_tblcontracts.year
    AND tgt_tblcontracts.idinstrument = src_tblcontracts.idinstrument

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
            @expirationdate,
            @cqgsymbol);

SET NOCOUNT OFF;