CREATE PROCEDURE [cqgdb].[test_SPO]
    @optionname VARCHAR (45), 
    @optionmonth CHAR (1), 
    @optionmonthint INT, 
    @optionyear INT, 
    @strikeprice DECIMAL (18,8), 
    @callorput CHAR (1), 
    @idinstrument BIGINT, 
    @expirationdate DATE,
    @cqgsymbol VARCHAR (45)
AS

MERGE INTO cqgdb.test_tbloptions as tgt_tbloptions
USING

    (SELECT * FROM [cqgdb].test_tblcontracts WHERE month = @optionmonth AND year = @optionyear)
    AS src_tbloptions
    ON tgt_tbloptions.idcontract = src_tbloptions.idcontract
    AND tgt_tbloptions.optionmonthint = @optionmonthint
    AND tgt_tbloptions.optionyear = @optionyear
    AND tgt_tbloptions.strikeprice = @strikeprice
    AND tgt_tbloptions.callorput = @callorput
    AND tgt_tbloptions.idinstrument = @idinstrument 

WHEN MATCHED THEN
UPDATE
    SET 
    optionname = @optionname,
    cqgsymbol = @cqgsymbol,
    expirationdate = @expirationdate

WHEN NOT MATCHED THEN
    INSERT     
    (optionname,
    optionmonth,
    optionmonthint,
    optionyear,
    strikeprice,
    callorput,
    idinstrument,
    expirationdate,
    idcontract,
    cqgsymbol)
    VALUES (@optionname, 
    @optionmonth, 
    @optionmonthint,
    @optionyear, 
    @strikeprice, 
    @callorput,
    @idinstrument, 
    @expirationdate,
    src_tbloptions.idcontract, 
    @cqgsymbol);
SET NOCOUNT ON;