using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectTemplate
{
    public class Account
    {
        //this is just a container for all info related
        //to an account.  We'll simply create public class-level
        //variables representing each piece of information!
        public int id;
        public string userId;
        public string pass;
        public string department;
        public int supervisor;
        public string issupervisor;
    }
}