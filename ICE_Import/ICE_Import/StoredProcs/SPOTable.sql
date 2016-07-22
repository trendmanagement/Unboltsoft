CREATE PROCEDURE [cqgdb].[SPOTable]
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

UPDATE temp set temp.idcontract = tblcontracts.idcontract from tblcontracts INNER JOIN temp ON
tblcontracts.idinstrument = temp.idinstrument AND
tblcontracts.monthint = temp.monthforfuture AND
tblcontracts.year= temp.yearforfuture

MERGE INTO cqgdb.tbloptions as tgt
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