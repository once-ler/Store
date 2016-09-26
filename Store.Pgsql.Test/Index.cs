﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSpec;
using Store.Interfaces;
using Store.Models;
using Store.IoC;
using Store.Pgsql;
using Store.Pgsql.Test.fixtures.Models;
using Store.Pgsql.Test.fixtures.Clients;

namespace Store.Pgsql.Test {

  class describe_store_pgsql : nspec {

    string foo_bar_1 = "foo bar 1";
    string foo_bar_2 = "foo bar 2";

    void before_each() {
      this.dbContext = new DBContext { server = "127.0.0.1", port = 5432, database = "pccrms", userId = "editor", password = "editor" };
      ServiceProvider.Instance.Singleton<IVersionControlManager>(() => new VersionControlManager(this.dbContext));
      ServiceProvider.Instance.Singleton<PersonnelClient<Personnel>>(() => new PersonnelClient<Personnel>(this.dbContext));
      ServiceProvider.Instance.Singleton<DroidClient<Droid>>(() => new DroidClient<Droid>(this.dbContext));
      ServiceProvider.Instance.Singleton<HumanClient<Human>>(() => new HumanClient<Human>(this.dbContext));
      ServiceProvider.Instance.Singleton<RebelAllianceClient<RebelAlliance>>(() => new RebelAllianceClient<RebelAlliance>(this.dbContext));
      ServiceProvider.Instance.Singleton<EmpireClient<Empire>>(() => new EmpireClient<Empire>(this.dbContext));
    }

    void describe_ioc_service_provider() {

      it["db context should not be null"] = () => dbContext.should_not_be(null);
      it["version control manager should not be null"] = () => ServiceProvider.Instance.GetService<IVersionControlManager>().should_not_be(null);
      it["droid client should not be null"] = () => ServiceProvider.Instance.GetService<DroidClient<Droid>>().should_not_be(null);
      it["human client should not be null"] = () => ServiceProvider.Instance.GetService<HumanClient<Human>>().should_not_be(null);
      it["rebel alliance client should not be null"] = () => ServiceProvider.Instance.GetService<RebelAllianceClient<RebelAlliance>>().should_not_be(null);
      it["empire client should not be null"] = () => ServiceProvider.Instance.GetService<EmpireClient<Empire>>().should_not_be(null);

      describe["setting multiple instances of same type"] = () => {
        before = () => {
          ServiceProvider.Instance.Singleton<DroidClient<Droid>>(() => new DroidClient<Droid>(this.dbContext));
          ServiceProvider.Instance.Singleton<DroidClient<Droid>>(() => new DroidClient<Droid>(this.dbContext));
        };
        it["should not throw an error"] = () => Console.WriteLine("this message safely displayed after multiple calls to create same type");
      };

      describe["getting multiple instances of same type"] = () => {
        before = () => {
          this.droidClientA = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
          this.droidClientB = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
        };
        it["both instances should be the same"] = () => this.droidClientA.should_be_same(this.droidClientB);
      };
    }

    void describe_version_control_manager() {
      IVersionControlManager p = null;
      
      before = () => {
        p = ServiceProvider.Instance.GetService<IVersionControlManager>();
        this.versions = p.getVersionControls();
      };
      it["can get a list of version controls"] = () => versions.should_not_be_null();
      
      describe["create 1 new version control if there are none"] = () => {
        before = () => {
          versions = p.getVersionControls();
        };
        act = () => {
          if (versions.Count() == 0)
            newVc = p.createNewVersionControl(foo_bar_1);
          else
            newVc = versions.FirstOrDefault(d => d.name == foo_bar_1);
        };
        it["new version control should not be null"] = () => newVc.should_not_be_null();
        it["new version control should have same name as inputted"] = () => newVc.should(d => d.name == foo_bar_1);
        it["version control list has at least one item"] = () => versions.Count().should_be_greater_than(0);        
      };

      describe["can create a new version control from an existing version control"] = () => {
        before = () => {
          versions = p.getVersionControls();
          newVc = versions.FirstOrDefault(d => d.name == foo_bar_1);
          newVc2 = versions.FirstOrDefault(d => d.name == foo_bar_2);
        };
        act = () => {
          if (newVc2 == null) {            
            newVc2 = p.createNewVersionControlFromExisting(newVc.id, foo_bar_2);
          }            
        };
        it["new version control created from an existing one should not be null"] = () => newVc2.should_not_be_null();
        it["new version control created from an existing one should have same name as inputted"] = () => newVc2.should(d => d.name == foo_bar_2);
        it["version control list has at least two item"] = () => versions.Count().should_be_greater_than(1);
      };      
    }

    void describe_client() {
      describe["can fetch a list of Record type of Personnel"] = () => {
        int offset = 0;
        int limit = 10;
        before = () => {
          var p = ServiceProvider.Instance.GetService<IVersionControlManager>();
          versions = p.getVersionControls();
          newVc = versions.FirstOrDefault(d => d.name == foo_bar_1);
          personnelClient = ServiceProvider.Instance.GetService<PersonnelClient<Personnel>>();          
        };
        act = () => {
          personnelList = personnelClient.list(newVc.id, offset, limit);
        };
        it["list of Personnel should not be empty"] = () => personnelList.should_not_be_empty();
        it["list of Personnel should have at least one record"] = () => personnelList.Count().should_be_greater_than(0);
        it["list of should be a Record of type Personnel"] = () => personnelList.FirstOrDefault().should(d => d.GetType() == typeof(Record<Personnel>));
      };

      describe["can fetch a one type of Personnel"] = () => {
        before = () => {
          var p = ServiceProvider.Instance.GetService<IVersionControlManager>();
          versions = p.getVersionControls();
          newVc = versions.FirstOrDefault(d => d.name == foo_bar_1);
          personnelClient = ServiceProvider.Instance.GetService<PersonnelClient<Personnel>>();
        };
        act = () => {
          personnel = personnelClient.one<Personnel>(newVc.id, "id", "taoh02");
        };
        it["Personnel should not be null"] = () => personnel.should_not_be_null();
        it["returned type should be of type Personnel"] = () => personnel.should(d => d.GetType() == typeof(Personnel));
      };

      describe["can fetch a one Record type of Personnel"] = () => {
        before = () => {
          var p = ServiceProvider.Instance.GetService<IVersionControlManager>();
          versions = p.getVersionControls();
          newVc = versions.FirstOrDefault(d => d.name == foo_bar_1);
          personnelClient = ServiceProvider.Instance.GetService<PersonnelClient<Personnel>>();
        };
        act = () => {
          recordOfPersonnel = personnelClient.one<Record<Personnel>>(newVc.id, "id", "taoh02");
        };
        it["Record of Personnel should not be null"] = () => recordOfPersonnel.should_not_be_null();
        it["returned type should be a Record of type Personnel"] = () => recordOfPersonnel.should(d => d.GetType() == typeof(Record<Personnel>));
      };

      describe["can save a one type of Personnel"] = () => {
        before = () => {
          var p = ServiceProvider.Instance.GetService<IVersionControlManager>();
          versions = p.getVersionControls();
          newVc = versions.FirstOrDefault(d => d.name == foo_bar_1);
          personnelClient = ServiceProvider.Instance.GetService<PersonnelClient<Personnel>>();
        };
        act = () => {
          personnel = personnelClient.one<Personnel>(newVc.id, "id", "taoh02");
          personnel = personnelClient.save(newVc.id, personnel);
        };
        it["saved personnel should not be null"] = () => personnel.should_not_be_null();
        it["saved result returned type should be of type Personnel"] = () => personnel.should(d => d.GetType() == typeof(Personnel));
      };

      describe["can save a one Record type of Personnel"] = () => {
        before = () => {
          var p = ServiceProvider.Instance.GetService<IVersionControlManager>();
          versions = p.getVersionControls();
          newVc = versions.FirstOrDefault(d => d.name == foo_bar_1);
          personnelClient = ServiceProvider.Instance.GetService<PersonnelClient<Personnel>>();
        };
        act = () => {
          recordOfPersonnel = personnelClient.one<Record<Personnel>>(newVc.id, "id", "taoh02");
          recordOfPersonnel = personnelClient.save(newVc.id, recordOfPersonnel);
        };
        it["saved record of type Personnel should not be null"] = () => recordOfPersonnel.should_not_be_null();
        it["saved result returned type should be of a Record of type Personnel"] = () => recordOfPersonnel.should(d => d.GetType() == typeof(Record<Personnel>));
      };

    }

    private DBContext dbContext;
    private DroidClient<Droid> droidClientA;
    private DroidClient<Droid> droidClientB;
    private List<VersionControl> versions = new List<VersionControl>();
    private List<Record<Personnel>> personnelList = new List<Record<Personnel>>();
    private VersionControl newVc = null;
    private VersionControl newVc2 = null;
    private PersonnelClient<Personnel> personnelClient;
    private Personnel personnel;
    private Record<Personnel> recordOfPersonnel;
  }

  class Index {
    static void Main(string[] args) {
      /**
       * packages\nspec.1.0.7\tools\NSpecRunner.exe C:\cygwin64\home\htao\Store\Store.Pgsql.Test\bin\Debug\Store.Pgsql.Test.dll
       */
    }
  }
}

/*
  Notes
  =====
  before(:all) runs the block one time before all of the examples are run.
  before(:each) runs the block one time before each of your specs in the file

  before(:all) sets the instance variables @admission, @project, @creative, @contest_entry one time before all of the it blocks are run.
  However, :before(:each) resets the instance variables in the before block every time an it block is run.
*/