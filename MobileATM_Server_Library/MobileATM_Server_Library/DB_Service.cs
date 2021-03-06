﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;

namespace MobileATM_Server_Library
{
    class DB_Service
    {
        static private DbConnection connection;
        static private DbProviderFactory factory;

        public DB_Service()
        {
            string provider = ConfigurationManager.AppSettings["provider"];
            string connectionString = ConfigurationManager.AppSettings["connectionString"];
            factory = DbProviderFactories.GetFactory(provider);

            connection = factory.CreateConnection();
            if (connection == null)
            {
                Console.WriteLine("Connection error");
                Console.ReadLine();
                return;
            }

            connection.ConnectionString = connectionString;

            connection.Open();
            Console.WriteLine("Connection successfull");
        }

        private List<string> QueryDB(string query)
        {
            List<string> res = new List<string>();

            DbCommand command = factory.CreateCommand();

            if (command == null)
            {
                Console.WriteLine("Command error");
                Console.ReadLine();
                return null;
            }

            command.Connection = connection;
            command.CommandText = query;

            using (DbDataReader dataReader = command.ExecuteReader())
            {
                int i = 0;

                while (dataReader.Read())
                {
                    while (i < dataReader.FieldCount)
                    {
                        res.Add($"{dataReader[i]}");
                        i++;
                    }
                }
            }
            return res;
        }

        public string GetDetail(int id)
        {
            List<string> detail = QueryDB($"Select * from Detail where detail_id={id}");
            return detail[1] + ":" + detail[2].ToString();
        }

        public string CheckServiceStaff(int password)
        {
            string command = "Select * from ServiceStaff where password= " + $"{password}";
            List<string> res = QueryDB(command);
            if (res.Count != 0)
            {
                return "Exist";
            }
            return "Doesn't exist";
        }

        public List<string> GetServiceStaff(int password)
        {
            List<string> serviceStaffList = QueryDB("Select * from ServiceStaff where password= " + $"{password}");
            return serviceStaffList;
        }

        public string CheckClient(string number)
        {
            string command = "Select * from Client where account_id = " + $"{number}";
            List<string> res = QueryDB(command);
            if (res.Count != 0)
            {
                return "Exist";
            }
            return "Doesn't exist";
        }

        public List<string> GetClientInformation(string number)
        {
            string command = "Select * from Client where account_id = " + $"{number}";
            List<string> res = QueryDB(command);
            return res;
        }

        public List<string> GetAccountInformation(string number)
        {
            string command = "Select * from Account where account_id = " + $"{number}";
            List<string> res = QueryDB(command);
            return res;
        }

        public void AddTransaction(string operation, int id)
        {
            //string command = $"Insert into Transaction values (transaction_type='{operation}', transaction_date='{DateTime.Now.Day}', account_id='{id}')";
            //QueryDB(command);
        }

        public void UpdateDB(string commandText)
        {
            DbCommand command = factory.CreateCommand();
            command.Connection = connection;
            command.CommandText = commandText.Replace(',', '.');
            command.ExecuteNonQuery();
        }
    }
}
