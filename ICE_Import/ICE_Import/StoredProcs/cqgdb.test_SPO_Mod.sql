CREATE PROCEDURE [cqgdb].[test_SPO_Mod]
    @optionname VARCHAR (45), 
    @optionmonth CHAR (1), 
    @optionmonthint INT, 
    @optionyear INT, 
    @strikeprice FLOAT (53), 
    @callorput CHAR (1), 
    @idinstrument BIGINT, 
    @cqgsymbol VARCHAR (45)
AS

MERGE INTO cqgdb.test_tbloptions as tgt_tbloptions
USING
    (SELECT * FROM [cqgdb].test_tblcontracts WHERE month = @optionmonth AND year = @optionyear)
    AS src_tblcontract

    ON tgt_tbloptions.idcontract = src_tblcontract.idcontract
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
    expirationdate = src_tblcontract.expirationdate

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
    VALUES 
    (@optionname, 
    @optionmonth, 
    @optionmonthint,
    @optionyear, 
    @strikeprice, 
    @callorput,
    @idinstrument, 
    src_tblcontract.expirationdate,
    src_tblcontract.idcontract,
    @cqgsymbol);
SET NOCOUNT ON;