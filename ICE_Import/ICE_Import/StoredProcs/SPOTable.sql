CREATE PROCEDURE [cqgdb].[SPOTable]
	@option OptionsType READONLY
AS

CREATE TABLE temp
(
    idcontract int,
    optionname varchar(45),
    optionmonth char,
    optionmonthint int,
    optionyear int,
    strikeprice float,
    callorput char,
    idinstrument int,
    expirationdate date,
    cqgsymbol varchar(45)
)

INSERT INTO temp
(
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
tblcontracts.month = temp.optionmonth AND
tblcontracts.year= temp.optionyear

MERGE INTO cqgdb.tbloptions as tgt
USING temp AS src
    ON tgt.idcontract = src.idcontract
    AND tgt.optionmonthint = src.optionmonthint 
    AND tgt.optionyear = src.optionyear
    AND tgt.strikeprice = src.strikeprice
    AND tgt.callorput = src.callorput
    AND tgt.idinstrument = src.idinstrument 

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
