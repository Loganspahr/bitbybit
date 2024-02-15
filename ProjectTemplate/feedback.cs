using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectTemplate
{
    public class Feedback
    {
        //this is just a container for all info related
        //to an account.  We'll simply create public class-level
        //variables representing each piece of information!
        public int id;
        public string question;
        public string answer;
        public string problemArea;
        public string complaint;
        public string suggestion;
        public string expiryDate;
        public int numAnswers;
        public int reviewed;
        public int unreviewed;
        public bool isExpired;
        public string questionType;
    }
}