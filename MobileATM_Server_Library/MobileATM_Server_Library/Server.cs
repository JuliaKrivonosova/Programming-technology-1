﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MobileATM_Server_Library
{
    public class Server
    {
        private IPHostEntry ipHost;
        private IPAddress ipAddr;
        private IPEndPoint ipEndPoint;
        private Client client = null;
        private ServiceStaff serviceStaff;
        private DB_Service db;
        private Socket handler;


        public void StartWorking()
        {
            // Создаем сокет Tcp/Ip
            Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);


            // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
            try
            {
                sListener.Bind(ipEndPoint);
                sListener.Listen(10);
                // Начинаем слушать соединения
                while (true)
                {
                    Console.WriteLine("Ожидаем соединение через порт {0}", ipEndPoint);

                    // Программа приостанавливается, ожидая входящее соединение
                    handler = sListener.Accept();

                    WorkingWithClient(handler);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }

        public Server()
        {
            db = new DB_Service();
            // Устанавливаем для сокета локальную конечную точку
            ipHost = Dns.GetHostEntry("localhost");
            ipAddr = ipHost.AddressList[0];
            ipEndPoint = new IPEndPoint(ipAddr, 11000);
        }

        private void WorkingWithClient(Socket handler)
        {
            bool done = false;

            while (!done)
            {
                string reply = GetData(handler);

                if (reply != "Successfuly closed socket")
                {
                    byte[] msg = Encoding.UTF8.GetBytes(reply);
                    handler.Send(msg);
                }
                else
                {
                    done = true;
                }
            }
        }

        private string GetData(Socket handler)
        {
            string messege = null;
            string res = "Error";

            // Мы дождались клиента, пытающегося с нами соединиться

            byte[] bytes = new byte[1024];
            int bytesRec = handler.Receive(bytes);

            messege += Encoding.UTF8.GetString(bytes, 0, bytesRec);

            // Показываем данные на консоли
            Console.Write("Полученный текст: " + messege + "\n\n");

            string[] data = messege.Split(';');

            int operation = Convert.ToInt32(data[0]);

            switch (operation)
            {
                case 0:
                    {
                        try
                        {
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                            res = "Successfuly closed socket";
                            Console.WriteLine("Successfuly closed socket");
                            client = null;
                            Console.WriteLine("Client was deleted");
                            handler = null;
                        }
                        catch (ObjectDisposedException ex)
                        {
                            Console.WriteLine(ex);
                            res = "error: Already closed";
                        }
                        break;
                    }
                case 1:
                    {
                        res = CheckClient(data[1]);
                        break;
                    }
                case 2:
                    {
                        res = client.GetBalance().ToString();
                        break;
                    }
                case 3:
                    {
                        res = Withdraw(data[1]);
                        break;
                    }
                case 4:
                    {
                        res = Deposit(data[1]);
                        break;
                    }
                case 5:
                    {
                        res = GetCondition();
                        break;
                    }
                case 6:
                    {
                        SetCondition(data[1]);
                        res = "Changed";
                        break;
                    }

                case 7:
                    {
                        res = CheckServiceStaff(Convert.ToInt32(data[1]));
                        break;
                    }
            }
            return res;
        }

        private string CheckClient(string number)
        {
            string res = "Error";
            if (number != null)
            {
                res = db.CheckClient(number);
            }
            if (res == "Exist")
            {
                client = CreateClient(number);
                Console.WriteLine("Client has created");
            }
            return res;
        }

        private string Deposit(string amount)
        {
            db.AddTransaction("deposit", client.GetId());
            var res = client.account.Deposit(Convert.ToDouble(amount), client.GetId());

            if (res == "OK")
            {
                db.UpdateDB($"Update Account set balance='{client.account.GetBalance()}' where account_id='{client.account.GetNumber()}'");
            }

            return res;
        }

        private string Withdraw(string amount)
        {
            db.AddTransaction("withdraw", client.GetId());
            var res = client.account.Withdraw(Convert.ToDouble(amount), client.GetId());

            if (res == "OK")
            {
                db.UpdateDB($"Update Account set balance='{client.account.GetBalance()}' where account_id='{client.account.GetNumber()}'");
            }

            return res;
        }

        private Client CreateClient(string num)
        {
            List<string> dataC = db.GetClientInformation(num);
            List<string> dataA = db.GetAccountInformation(num);
            Account account;
            if (Convert.ToInt32(dataA[2]) == 0)
            {
                account = new DebitCard(dataA[0], Convert.ToDouble(dataA[1]));
                Console.WriteLine("Debit card created");
            }
            else
            {
                account = new CreditCard(dataA[0], Convert.ToDouble(dataA[1]));
                Console.WriteLine("Credit card created");
            }
            client = new Client(Convert.ToInt32(dataC[0]), dataC[1], account);

            return client;
        }

        private string GetCondition()
        {
            return db.GetDetail(1) + "|" + db.GetDetail(2);
        }

        private void SetCondition(string cond)
        {
            string name = cond.Split('|')[0].Split(':')[0];
            float res = float.Parse(cond.Split('|')[0].Split(':')[1]);
            db.UpdateDB($"Update Detail set detail_resource='{res}' where name='{name}'");

            name = cond.Split('|')[1].Split(':')[0];
            res = float.Parse(cond.Split('|')[1].Split(':')[1]);
            db.UpdateDB($"Update Detail set detail_resource='{res}' where name='{name}'");
        }

        private string CheckServiceStaff(int password)
        {
            string res = "Error";
            if (password < 10000)
            {
                res = db.CheckServiceStaff(password);
            }
            else
            {
                res = "Incorrect pin";
            }
            if (res == "Exist")
            {
                //serviceStaff = CreateServiceStaff(password);
                Console.WriteLine("Service Staff was created");
            }
            return res;
        }

        private ServiceStaff CreateServiceStaff(int password)
        {
            List<string> dataSS = db.GetServiceStaff(password);

            serviceStaff = new ServiceStaff(Convert.ToInt32(dataSS[0]), dataSS[1], password);

            return serviceStaff;
        }
    }
}
