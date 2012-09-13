using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PaymentDelete.zuora;

namespace PaymentDelete
{
    class Program
    {

        static string USERNAME = "";
        static string PASSWORD = "";
        static string ENDPOINT = "https://apisandbox.zuora.com/apps/services/a/39.0";
        private zuora.ZuoraService binding;

        public Program()
        {
            binding = new zuora.ZuoraService();
            binding.Url = ENDPOINT;

        }

        static void Main(string[] args)
        {
            Program p = new Program();
            p.login();
            List<Payment> pList = p.getPaymentsForCreatedById("4028e696380c01c401382ad088fe6344");
            Console.WriteLine("Count: " + pList.Count());
            List<Payment> updatedPayments = new List<Payment>();
            List<String> delMeIdsList = new List<String>();
            List<String> ids = new List<String>();
            foreach (Payment pay in pList)
            {
                pay.Status = "Canceled";
                updatedPayments.Add(pay);
                if (updatedPayments.Count > 49)
                {
                    ids = p.update(updatedPayments.ToArray());
                    foreach (String s in ids)
                    {
                        Console.WriteLine("ids: " + s);
                        delMeIdsList.Add(s);
                    }

                    updatedPayments.Clear();
                }
            }
            ids = p.update(updatedPayments.ToArray());
            foreach (String s in ids)
            {
                Console.WriteLine("ids: " + s);
                delMeIdsList.Add(s);
            }
            Console.WriteLine("DelMeIdsCount: " + delMeIdsList.Count);
            List<String> delMe = new List<String>();
            foreach (String pay in delMeIdsList)
            {
                
                delMe.Add(pay);
                Console.WriteLine("DelMe count: " + delMe.Count());
                if (delMe.Count > 49)
                {
                    Console.WriteLine("Deleting");
                    p.delete("Payment", delMe.ToArray());
                    delMe.Clear();
                }
            }
            p.delete("Payment", delMe.ToArray());
            Console.WriteLine("Done...");
            Console.ReadLine();

            
        }
        //login
        private bool login()
        {
            try
            {
                //execute the login placing the results  
                //in a LoginResult object 
                zuora.LoginResult loginResult = binding.login(USERNAME, PASSWORD);

                //set the session id header for subsequent calls 
                binding.SessionHeaderValue = new zuora.SessionHeader();
                binding.SessionHeaderValue.session = loginResult.Session;

                //reset the endpoint url to that returned from login 
                // binding.Url = loginResult.ServerUrl;

                Console.WriteLine("Session: " + loginResult.Session);
                Console.WriteLine("ServerUrl: " + loginResult.ServerUrl);

                return true;
            }
            catch (Exception ex)
            {
                //Login failed, report message then return false 
                Console.WriteLine("Login failed with message: " + ex.Message);
                return false;
            }
        }
        private string create(zObject acc)
        {
            SaveResult[] result = binding.create(new zObject[] { acc });
            return result[0].Id;
        }
        private List<Payment> getPaymentsForCreatedById(String accId)
        {
            List<Payment> output = new List<Payment>();
            QueryResult qResult = binding.query("SELECT id, status FROM payment WHERE createdById = '" + accId + "'");
            foreach (zObject z in qResult.records)
            {
                output.Add((Payment)z);
            }
            return output;
        }
        private Account queryAccount(string accId)
        {
            QueryResult qResult = binding.query("SELECT id, name, accountnumber FROM account WHERE id = '" + accId + "'");
            Account rec = (Account)qResult.records[0];
            return rec;
        }

        private List<String> update(zObject[] input)
        {
            List<String> ids = new List<String>();
            SaveResult[] result = binding.update(input);
            foreach (SaveResult sr in result)
            {
                if (sr.Success == true)
                {
                    ids.Add(sr.Id);

                }
                else
                {
                    Console.WriteLine(sr.Errors[0].Message);
                }
            }
            return ids;
        }

        private List<String> delete(String type, String[] ids)
        {

            DeleteResult[] result = binding.delete(type, ids);
            List<String> results = new List<String>();
            foreach (DeleteResult dr in result)
            {
                if (dr.success)
                {
                    Console.WriteLine("Delete Success!");
                    results.Add(dr.id);
                }
                else
                {
                    Console.WriteLine("Delete Failure! " + dr.errors[0].Message);
                    results.Add(dr.errors[0].Message);
                }
            }
            return results;

        }
    }
}
