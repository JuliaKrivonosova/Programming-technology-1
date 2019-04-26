﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileATM_Library
{
    public class Client
    { 
        private string name;
        private string surname;
        private List<Account> accountList;

        public string Name
        {
            get
            {
                return name;
            }

            private set
            {
                name = value;
            }
        }

        public string Surname
        {
            get
            {
                return surname;
            }

            set
            {
                surname = value;
            }
        }

        public Client(string name, string surname)
        {
            accountList = new List<Account>();
            this.name = name;
            this.surname = surname;
        }

        public void addAccount() { }
    }
}