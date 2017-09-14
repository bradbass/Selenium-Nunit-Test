using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace eMobileSeleniumNunitTest
{
    class EServiceTest
    {
        private IWebDriver driver;

        public EServiceTest(IWebDriver driver)
        {
            // TODO: Complete member initialization
            this.driver = driver;
        }

        public void EServiceTestCase()
        {
            EMobileSeleniumNunitTest ts = new EMobileSeleniumNunitTest();

            var wait2 = 2000;
            //add explicit wait to avoid issues where the browser is a bit slow so the test fails to find the required element
            WebDriverWait eWait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            //add an implicit wait
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(2));

            //eService login
            driver.Url = "https://constqa124.jonasportal.com/eservice/";

            //find and click login button
            driver.FindElement(By.XPath("//a[@href='#login_view']")).Click();

            //wait for login page
            Thread.Sleep(4000);
            IWebElement loginPage = eWait.Until<IWebElement>((d) =>
            {
                return d.FindElement(By.Id("UserIDLogin"));
            });

            //find and fill out username and password
            driver.FindElement(By.Id("UserIDLogin")).SendKeys("braba");
            Thread.Sleep(wait2);
            driver.FindElement(By.Id("pwdLogin")).SendKeys("'");
            Thread.Sleep(wait2);

            //find and click login button
            driver.FindElement(By.Id("SubmitLogin")).Click();

            //wait for home page
            Thread.Sleep(4000);
            IWebElement homeView = eWait.Until<IWebElement>((d) =>
            {
                return d.FindElement(By.Id("home_view"));
            });

            //find and click on work orders tab and select create new wo
            driver.FindElement(By.Id("wo_list_menu")).Click();
            Thread.Sleep(wait2);
            driver.FindElement(By.XPath("//li[@goto='#new_wo_view']")).Click();

            //wait for new wo page
            Thread.Sleep(4000);
            IWebElement newWOView = eWait.Until<IWebElement>((d) =>
            {
                return d.FindElement(By.Id("new_wo_view"));
            });

            //find and fill out Problem Description box
            driver.FindElement(By.Id("problem")).SendKeys("This is a test description");
            Thread.Sleep(wait2);

            //find and fill out who called box
            driver.FindElement(By.Id("whocalled")).SendKeys("QA-Tester");
            Thread.Sleep(wait2);

            //find and click on spare fields tab
            driver.FindElement(By.XPath("//li[@aria-controls='newwo_tabstrip-2']")).Click();
            Thread.Sleep(wait2);

            //fill out a couple spare fields
            driver.FindElement(By.Name("field2_input")).SendKeys("1");
            Thread.Sleep(wait2);
            driver.FindElement(By.Name("field3_input")).SendKeys("1.5 lbs");
            Thread.Sleep(wait2);
            driver.FindElement(By.Name("field4_input")).SendKeys("test desc");
            Thread.Sleep(wait2);
            driver.FindElement(By.Name("field5_input")).SendKeys("tets status");
            Thread.Sleep(wait2);
            driver.FindElement(By.Name("field6_input")).SendKeys("test truck");
            Thread.Sleep(wait2);

            //find and click on Documents tab
            driver.FindElement(By.XPath("//li[@aria-controls='newwo_tabstrip-3']")).Click();
            Thread.Sleep(wait2);

            //TODO: add document

            //find and click on submit
            driver.FindElement(By.Id("SubmitNewWO")).Click();

            //wait for new wo created confirmation box
            Thread.Sleep(4000);
            IWebElement newWOCreated = eWait.Until<IWebElement>((d) =>
            {
                return d.FindElement(By.Id("closeNewWoOkBtn"));
            });

            //click on Close on the new wo submitted box
            driver.FindElement(By.Id("closeNewWoOkBtn")).Click();

            //wait for new wo created view
            Thread.Sleep(4000);
            IWebElement newWOCreatedView = eWait.Until<IWebElement>((d) =>
            {
                return d.FindElement(By.Id("gridOnlineWO"));
            });
            Thread.Sleep(5000);
        }
    }
}
