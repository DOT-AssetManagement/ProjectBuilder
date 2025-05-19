using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;



namespace PBLogic
{
    public class BulkInserter : IDisposable
    {
        protected static object syncRoot = new Object();

        protected DataTable _dt = null;
        protected SqlConnection _conn = null;
        protected SqlBulkCopy _bc = null;
        protected int _batchSize = 100000;

        public DataTable DTable
        {
            get
            {
                return _dt;
            }
        }

        public void Dispose()
        {
            if (_bc != null && _dt != null && _conn != null)
            {
                Flush();
            }
        }

        /// <summary>
        /// Configures internal objects
        /// </summary>
        /// <param name="conn">SQL connection</param>
        /// <param name="dbTableName">Name of the database table of the Staging family.  The table MUST have PrlPrSKU column.</param>
        /// <param name="dtTableName">Name for the virtual data table</param>
        /// <param name="batchSize">Number of rows, upon reaching which data is automatically flush.  May be null, in which case default value is used.</param>
        /// <param name="timeoutSec">Timeout (seconds), may be null</param>
        /// <param name="errorMessage">(out) error message.</param>
        /// <returns>True on success, False on failure.</returns>
        public Boolean Configure(SqlConnection conn, String dbTableName, String dtTableName, Int32? batchSize, Int32? timeoutSec, out String errorMessage)
        {
            Boolean ok = true;
            errorMessage = null;

            try
            {
                _conn = conn;
                if (_conn.State != ConnectionState.Open)
                {
                    _conn.Open();
                }

                _dt = new DataTable(dtTableName);

                String sql = "SELECT TOP 1 * FROM " + dbTableName + " WITH (NOLOCK)";
                using (SqlDataAdapter a = new SqlDataAdapter(sql, _conn))
                {
                    a.Fill(_dt);
                    _dt.Rows.Clear();
                }

                _bc = new SqlBulkCopy(_conn);
                _bc.DestinationTableName = dbTableName;
                _bc.ColumnMappings.Clear();

                foreach (DataColumn col in _dt.Columns)
                {
                    _bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping(col.ColumnName, col.ColumnName));
                }

                if (batchSize.HasValue && batchSize.Value >= 0)
                    _batchSize = batchSize.Value;

                if (timeoutSec.HasValue && timeoutSec.Value >= 0)
                    _bc.BulkCopyTimeout = timeoutSec.Value;
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
            }
            return (ok);
        }


        /// <summary>
        /// Creates and returns the DataRow object for the table
        /// </summary>
        /// <returns>DataRow object</returns>
        public DataRow NewRow()
        {
            return _dt.NewRow();
        }


        /// <summary>
        /// Removes all rows from the table
        /// </summary>
        public void Clear()
        {
            _dt.Rows.Clear();
        }

        /// <summary>
        /// Adds the new row and flushes upon reaching the batch size
        /// </summary>
        /// <param name="r">DataRow object to be added</param>
        /// <param name="errorMessage">(out) error message</param>
        /// <returns>True if ok, False on failure</returns>
        public Boolean AddRow(DataRow r, out String errorMessage)
        {
            Boolean ok = true;
            errorMessage = null;

            try
            {
                _dt.Rows.Add(r);
                if (_batchSize > 0 && _dt.Rows.Count >= _batchSize)
                {
                    Flush();
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
            }
            return (ok);
        }

        /// <summary>
        /// Flushes data table to the database
        /// </summary>
        public void Flush()
        {
            try
            {
                if (_dt.Rows.Count > 0)
                {
                    lock (syncRoot)
                    {
                        if (_conn.State != ConnectionState.Open)
                            _conn.Open();

                        _bc.BulkCopyTimeout = 60;
                        _bc.WriteToServer(_dt);
                        _dt.Rows.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("BulkInserter.Flush - {0}", ex.Message));
            }
        }

        public void FlushAsync()
        {
            new System.Threading.Thread(() => Flush()).Start();
        }
    }

}
