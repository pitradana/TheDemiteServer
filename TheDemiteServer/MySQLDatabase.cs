using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace TheDemiteServer
{
    class MySQLDatabase
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        public MySQLDatabase()
        {
            DBInit();
        }

        private void DBInit()
        {
            //server = "localhost";
            //database = "thedemitedb";
            //uid = "root";
            //password = "";

            server = "167.205.7.233";
            database = "thesis_demite";
            uid = "demite";
            password = "ThesisDemite1907";

            string connectionString = "SERVER = " + server + ";" + "DATABASE = " + database + ";" + "UID= " + uid + ";" + "PASSWORD = " + password + ";";

            connection = new MySqlConnection(connectionString);
        }

        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch(MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch(MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        public int Insert (string tableName, string culumns, string values)
        {
            int affected = -1;
            string query = "INSERT INTO " + tableName + " (" + culumns + ") VALUES(" + values + ")";

            if(this.OpenConnection())
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                affected = command.ExecuteNonQuery();
                this.CloseConnection();
            }

            return affected;
        }

        public int Update(string tableName, string data, string terms)
        {
            int affected = -1;
            string query = "UPDATE " + tableName + " SET " + data + " WHERE " + terms;

            if (this.OpenConnection())
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                affected = command.ExecuteNonQuery();
                this.CloseConnection();
            }

            return affected;
        }

        public void Delete(string tableName, string terms)
        {
            string query = "DELETE FROM " + tableName + " WHERE " + terms;
            if(this.OpenConnection())
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        public int Count(string tableName, string terms)
        {
            int count = -1;
            string query = "SELECT Count(*) FROM " + tableName + " WHERE " + terms;

            if(this.OpenConnection())
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                count = int.Parse(command.ExecuteScalar() + "");
                this.CloseConnection();

                return count;
            }
            else
            {
                return count;
            }
        }

        public int CreateNewAccount(string firstName, string lastName, string username, string password, string email)
        {
            int affected = -1;

            int count = this.Count("user", "username='" + username + "'");
            if(count == 0)
            {
                affected = this.Insert("user", "username, password, is_active, first_name, last_name, email", "'" + username + "', '" + password + "', 0, '" + firstName + "', '" + lastName + "', '" + email + "'");
            }
            else
            {
                affected = -2;
            }

            return affected;
        }

        public int FindUserIdByEmail(string email)
        {
            int id = -1;
            string query = "SELECT id FROM User Where email='" + email + "'";

            if(this.OpenConnection())
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                while(reader.Read())
                {
                    id = (int)reader["id"];
                }

                reader.Close();
                this.CloseConnection();
            }

            return id;
        }

        public string FindEmail(string username)
        {
            string email = "";
            string query = "SELECT email FROM User WHERE username='" + username + "'";

            if(this.OpenConnection())
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                while(reader.Read())
                {
                    email = (string)reader["Email"];
                }

                reader.Close();
                this.CloseConnection();
            }

            return email;
        }
    }
}
