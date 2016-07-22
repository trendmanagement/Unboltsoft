CREATE PROCEDURE [cqgdb].[test_SPOTable]
    @option OptionsType READONLY
AS

CREATE TABLE temp
(
	monthforfuture int,
	yearforfuture int,
    idcontract int,
    optionname varchar(45),
    optionmonth char,
    optionmonthint int,
    optionyear int,
    strikeprice decimal(18,8),
    callorput char,
    idinstrument int,
    expirationdate date,
    cqgsymbol varchar(45)
)

INSERT INTO temp
(
	monthforfuture,
	yearforfuture,
    optionname,
    optionmonth,
    optionmonthint,
    optionyear,
    strikeprice,
    callorput,
    idinstrument,
    expirationdate,
    cqgsymbol
)
SELECT
	monthforfuture,
	yearforfuture,
    optionname,
    optionmonth,
    optionmonthint,
    optionyear,
    strikeprice,
    callorput,
    idinstrument,
    expirationdate,
    cqgsymbol
FROM @option

UPDATE temp set temp.idcontract = test_tblcontracts.idcontract from test_tblcontracts INNER JOIN temp ON
test_tblcontracts.idinstrument = temp.idinstrument AND
test_tblcontracts.monthint = temp.monthforfuture AND
test_tblcontracts.year= temp.yearforfuture

MERGE INTO cqgdb.test_tbloptions as tgt
USING temp AS src
    ON tgt.optionname = src.optionname

WHEN MATCHED THEN
UPDATE
    SET 
    optionname = src.optionname,
    cqgsymbol = src.cqgsymbol,
    expirationdate = src.expirationdate

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
    (src.optionname, 
    src.optionmonth, 
    src.optionmonthint,
    src.optionyear, 
    src.strikeprice, 
    src.callorput,
    src.idinstrument, 
    src.expirationdate,
    src.idcontract, 
    src.cqgsymbol);
DROP TABLE temp;
SET NOCOUNT ON;