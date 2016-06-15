using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ICE_Import
{


    class TableStoredProcedureTest
    {
        public SqlConnectionStringBuilder connString1Builder;

        public void setupSqlConnectionString()
        {
            connString1Builder = new SqlConnectionStringBuilder();
            connString1Builder.DataSource = "tcp:h9ggwlagd1.database.windows.net,1433";
            connString1Builder.InitialCatalog = "TMLDB";
            connString1Builder.Encrypt = true;
            connString1Builder.TrustServerCertificate = false;
            connString1Builder.UserID = "steve@h9ggwlagd1";
            connString1Builder.Password = "KYYAtv9P";
            connString1Builder.MultipleActiveResultSets = true;
            connString1Builder.Pooling = true;
            connString1Builder.MaxPoolSize = 90;
            connString1Builder.ConnectTimeout = 0;
        }

        public void ConnectDBAndExecuteQuerySynchronousWithTransaction(
            DataTable optionDataTableForUpsert,
            SqlConnectionStringBuilder connString1BuilderInternal)//, SqlConnection connection)
        {

            using (SqlConnection connection = new SqlConnection(connString1BuilderInternal.ToString()))
            {
                connection.Open();

                using (SqlTransaction trans = connection.BeginTransaction())
                {

                    using (SqlCommand command = new SqlCommand("cqgdb.sp_updateOrInsertTbloptionsInfoAndDataUpsertTable", connection, trans))
                    {

                        command.CommandTimeout = 0;

                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@tblOptionDataInput", optionDataTableForUpsert);

                        command.ExecuteNonQuery();

                    }
                    try
                    {
                        trans.Commit();
                    }
                    catch (Exception commitEx)
                    {
                        //trans.Rollback();
                        Console.WriteLine("didn't work " + commitEx); ;
                    }

                }

                connection.Close();
            }
        }

    }

    //below is the user-defined table type
    /*
     * CREATE TYPE cqgdb.OptionDataUpdateType AS TABLE(
	optionname VARCHAR (45),
	optionmonth CHAR (1),
	optionmonthint INT, 
	optionyear INT, 
	strikeprice FLOAT (53), 
	callorput CHAR (1), 
	idinstrument BIGINT, 
	expirationdate DATE, 
	idcontract BIGINT, 
	cqgsymbol VARCHAR (45),
	datetime DATE,
	price FLOAT(53),
	impliedvol FLOAT(53),
	timetoexpinyears FLOAT(53)
)

GO
*/

    //below is the stored procedure
    /*
    USE [TMLDB]
    GO
    SET ANSI_NULLS ON
    GO
    SET QUOTED_IDENTIFIER ON
    GO

    ALTER PROCEDURE [cqgdb].[sp_updateOrInsertTbloptionsInfoAndDataUpsertTable]

    @tblOptionDataInput OptionDataUpdateType READONLY
    
    AS

    SET NOCOUNT ON;
    CREATE TABLE #temp1
    (
    idoption INT, optionname VARCHAR (45), optionmonth CHAR (1), optionmonthint INT, 
    optionyear INT, strikeprice FLOAT (53), callorput CHAR (1), 
    idinstrument BIGINT, expirationdate DATE, idcontract BIGINT, cqgsymbol VARCHAR (45),
    datetime DATE,
    price FLOAT(53),
    impliedvol FLOAT(53),
    timetoexpinyears FLOAT(53)
    )

    INSERT INTO #temp1 (optionname, optionmonth, optionmonthint, 
	optionyear, strikeprice, callorput, 
	idinstrument, expirationdate, idcontract, cqgsymbol,
	datetime,
	price,
	impliedvol,
	timetoexpinyears) 
	SELECT optionname, optionmonth, optionmonthint, 
	optionyear, strikeprice, callorput, 
	idinstrument, expirationdate, idcontract, cqgsymbol,
	datetime,
	price,
	impliedvol,
	timetoexpinyears FROM @tblOptionDataInput;

    MERGE INTO cqgdb.tbloptions as tgttbloptions
    USING
    @tblOptionDataInput srctbloptions
    ON tgttbloptions.optionmonthint = srctbloptions.optionmonthint
	AND tgttbloptions.optionyear = srctbloptions.optionyear
	AND tgttbloptions.strikeprice = srctbloptions.strikeprice
	AND tgttbloptions.callorput = srctbloptions.callorput
	AND tgttbloptions.idinstrument = srctbloptions.idinstrument

    WHEN MATCHED THEN
    UPDATE
	SET idcontract = srctbloptions.idcontract,
	optionname = srctbloptions.optionname,
	cqgsymbol = srctbloptions.cqgsymbol,
	expirationdate = srctbloptions.expirationdate

    WHEN NOT MATCHED THEN

	INSERT (optionname,optionmonth,
	optionmonthint,optionyear,strikeprice,callorput,
	idinstrument,expirationdate,idcontract,cqgsymbol)
	VALUES (srctbloptions.optionname, srctbloptions.optionmonth , 
	srctbloptions.optionmonthint, srctbloptions.optionyear, 
	srctbloptions.strikeprice, srctbloptions.callorput,
	srctbloptions.idinstrument, srctbloptions.expirationdate,
	srctbloptions.idcontract, srctbloptions.cqgsymbol);


    DECLARE @idOptionTable AS cqgdb.OptionIdTableType;

    update #temp1 set #temp1.idOption = tbloptions.idoption
	
	FROM tbloptions
	INNER JOIN #temp1
	ON 
    tbloptions.optionmonthint = #temp1.optionmonthint
	AND tbloptions.optionyear = #temp1.optionyear
	AND tbloptions.strikeprice = #temp1.strikeprice
	AND tbloptions.callorput = #temp1.callorput
	AND tbloptions.idinstrument = #temp1.idinstrument;


    MERGE INTO cqgdb.tbloptiondata as tgttbloptiondata
    USING
	    #temp1 srctbloptiondata
        ON tgttbloptiondata.idoption = srctbloptiondata.idoption
        AND tgttbloptiondata.datetime = srctbloptiondata.datetime

    WHEN MATCHED THEN
    UPDATE

        SET price = srctbloptiondata.price,
        impliedvol = srctbloptiondata.impliedvol,
        timeToExpInYears = srctbloptiondata.timeToExpInYears

    WHEN NOT MATCHED THEN


        INSERT (idoption, datetime, price,
        impliedvol, timetoexpinyears)

        VALUES(srctbloptiondata.idoption, srctbloptiondata.datetime, srctbloptiondata.price,
        srctbloptiondata.impliedvol, srctbloptiondata.timetoexpinyears);

    DROP TABLE #Temp1;

    SET NOCOUNT OFF;


     * 
     */
}
