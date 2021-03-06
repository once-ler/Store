﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using Npgsql;
using NSpec;
using Store.Interfaces;
using Store.Models;
using Store.IoC;
using Store.Storage.Data;
using Store.Storage.Pgsql;
using Store.Storage.Pgsql.Test.fixtures.Models;
using Store.Storage.Pgsql.Test.fixtures.Clients;

namespace Store.Storage.Pgsql.Test {

  class describe_store_pgsql : nspec {

    string foo_bar_1 = "foo bar 1";
    string foo_bar_2 = "foo bar 2";
    string someVersion = "v$12345678";
    DBContext dbContext = new DBContext { server = "127.0.0.1", port = 5432, database = "pccrms", userId = "editor", password = "editor" };

    void before_each() {
      ServiceProvider.Instance.Singleton(() => new VersionControlManager(dbContext));
      ServiceProvider.Instance.Singleton<PersonnelClient<Personnel>>(() => new PersonnelClient<Personnel>(dbContext));
      ServiceProvider.Instance.Singleton<DroidClient<Droid>>(() => new DroidClient<Droid>(dbContext));
      ServiceProvider.Instance.Singleton<HumanClient<Human>>(() => new HumanClient<Human>(dbContext));
      ServiceProvider.Instance.Singleton<RebelAllianceClient<RebelAlliance>>(() => new RebelAllianceClient<RebelAlliance>(dbContext));
      ServiceProvider.Instance.Singleton<EmpireClient<Empire>>(() => new EmpireClient<Empire>(dbContext));
      ServiceProvider.Instance.Singleton<Client<EmpireExtended>>(() => new Client<EmpireExtended>(dbContext));
      ServiceProvider.Instance.Singleton<Client<DroidExtended>>(() => new Client<DroidExtended>(dbContext));
      ServiceProvider.Instance.Register("Empire", typeof(Empire));

      personnel = null;
      recordOfPersonnel = null;
      personnelClient = null;
      newVc = null;
      newVc2 = null;
    }

    void describe_ioc_service_provider() {

      it["db context should not be null"] = () => dbContext.should_not_be(null);
      it["version control manager should not be null"] = () => ServiceProvider.Instance.GetService<VersionControlManager>().should_not_be(null);
      it["droid client should not be null"] = () => ServiceProvider.Instance.GetService<DroidClient<Droid>>().should_not_be(null);
      it["human client should not be null"] = () => ServiceProvider.Instance.GetService<HumanClient<Human>>().should_not_be(null);
      it["rebel alliance client should not be null"] = () => ServiceProvider.Instance.GetService<RebelAllianceClient<RebelAlliance>>().should_not_be(null);
      it["empire client should not be null"] = () => ServiceProvider.Instance.GetService<EmpireClient<Empire>>().should_not_be(null);

      describe["setting multiple instances of same type"] = () => {
        before = () => {
          ServiceProvider.Instance.Singleton<DroidClient<Droid>>(() => new DroidClient<Droid>(dbContext));
          ServiceProvider.Instance.Singleton<DroidClient<Droid>>(() => new DroidClient<Droid>(dbContext));
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

      describe["getting empire type by string"] = () => {
        Type ty = null;
        act = () => {
          ty = ServiceProvider.Instance.GetType("Empire");
        };
        it["type should be type of Empire"] = () => ty.should_be_same(typeof(Empire));
      };
    }

    void describe_version_control_manager() {
      VersionControlManager p = null;
      
      before = () => {
        p = ServiceProvider.Instance.GetService<VersionControlManager>();
        this.versions = p.getVersionControls();
      };
      it["can get a list of version controls"] = () => versions.should_not_be_null();
      
      describe["create foo bar 1 version control if there are none"] = () => {
        before = () => {
          versions = p.getVersionControls().Where(d => d.name == foo_bar_1).ToList();
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
      
      describe["can explicitly create a version with user defined version id"] = () => {        
        before = () => {
          versions = p.getVersionControls();
          newVc = versions.FirstOrDefault(d => d.id == someVersion);
        };
        act = () => {
          if (newVc == null) {
            var p1 = ServiceProvider.Instance.GetService<VersionControlManager>();
            newVc = p1.createNewVersionControl(someVersion, "Some version name");
          }
        };
        it["new version id should have same id user provided"] = () => newVc.id.should_be(someVersion);
      };    
    }

    void describe_client() {
      describe["can fetch a list of Record type of Personnel"] = () => {
        int offset = 0;
        int limit = 10;
        before = () => {
          var p = ServiceProvider.Instance.GetService<VersionControlManager>();
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
          var p = ServiceProvider.Instance.GetService<VersionControlManager>();
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
          var p = ServiceProvider.Instance.GetService<VersionControlManager>();
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

      describe["can save one type of Personnel"] = () => {
        before = () => {
          var p = ServiceProvider.Instance.GetService<VersionControlManager>();
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

      describe["can save one Record type of Personnel"] = () => {
        before = () => {
          var p = ServiceProvider.Instance.GetService<VersionControlManager>();
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

    void describe_client_additional_tests() {
      describe["fetch will succeed even if version and store has not been created"] = () => {
        string someVersion = "v$12345678";
        string sql = "drop schema v$12345678 cascade";
        before = () => {
          var fac = new Factory<NpgsqlConnection>();
          var dbConnection = fac.Get(dbContext);
          CommandRunner runner = new CommandRunner(dbConnection);
          try {
            runner.Transact(new DbCommand[] { runner.BuildCommand(sql, null) });
          } catch (Exception err) {
            // Expect -> 3F000: schema "v$12345678" does not exist
            Console.WriteLine(err.Message);
          }
        };
        act = () => {
          var droidClient = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
          droid = droidClient.one<Droid>(someVersion, "id", "R2-D2");
        };

        it["droid should be null and no errors were thrown and unhandled"] = () => droid.should_be_null();
      };

      describe["can create version and store on demand"] = () => {
        string someVersion = "v$12345678";
        before = () => {
          var droidClient = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
          var droid = new Droid { id = "2-1B", name = "2-1B", ts = DateTime.Now };
          droidClient.save(someVersion, droid);
        };
        act = () => {
          var p = ServiceProvider.Instance.GetService<VersionControlManager>();
          versions = p.getVersionControls();
          newVc = versions.FirstOrDefault(d => d.id == someVersion);
        };
        it["new version control should be created if not exist"] = () => newVc.should_not_be_null();
      };

      describe["can associate a participant to an affiliation"] = () => {
        string someVersion = "v$12345678";
        before = () => {
          var droidClient = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
          var droid = new Droid { id = "2-1B", name = "2-1B", ts = DateTime.Now };
          droidClient.save(someVersion, droid);

          var empireClient = ServiceProvider.Instance.GetService<EmpireClient<Empire>>();
          var badRoster = new List<Participant> { new Participant { id = droid.id, party = droid } };
          var empire = new Empire { id = "empire", roster = badRoster, ts = DateTime.Now };
          empireClient.save(someVersion, empire);
        };
        act = () => {
          empireClient = ServiceProvider.Instance.GetService<EmpireClient<Empire>>();
          empire = empireClient.one<Empire>(someVersion, "id", "empire");
          var droidClient = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
          droid = droidClient.one<Droid>(someVersion, "id", "2-1B");
          empireClient.associate<Participant, Droid>(someVersion, empire.id, droid.id);
          empire = empireClient.one<Empire>(someVersion, "id", "empire");
        };
        it["the empire should not be null"] = () => empire.should_not_be_null();
        it["the droid should be a participant of the empire"] = () => empire.roster.FirstOrDefault(d => d.party.id == "2-1B").should_not_be_null();
      };

      describe["can associate a derived participant to an derived affiliation"] = () => {
        string someVersion = "v$12345678";
        int beforeAssociateCount = 0;
        int afterAssociateCount = 0;
        before = () => {
          var droidExtendedClient = ServiceProvider.Instance.GetService<Client<DroidExtended>>();
          var droidExtended = new DroidExtended { id = "2-1B", name = "2-1B", ts = DateTime.Now };
          droidExtendedClient.save(someVersion, droidExtended);

          var empireExtendedClient = ServiceProvider.Instance.GetService<Client<EmpireExtended>>();
          var badRoster = new List<ParticipantExtended> { new ParticipantExtended { id = droidExtended.id, party = droidExtended } };
          var empire = new EmpireExtended { id = "empire", roster = badRoster, ts = DateTime.Now };
          empireExtendedClient.save(someVersion, empire);
        };
        act = () => {
          var empireExtendedClient = ServiceProvider.Instance.GetService<Client<EmpireExtended>>();
          empireExtended = empireExtendedClient.one<EmpireExtended>(someVersion, "id", "empire");
          beforeAssociateCount = empireExtended.roster.Count();
          var droidExtendedClient = ServiceProvider.Instance.GetService<Client<DroidExtended>>();
          droidExtended = droidExtendedClient.one<DroidExtended>(someVersion, "id", "2-1B");
          empireExtendedClient.associate<ParticipantExtended, DroidExtended>(someVersion, empireExtended.id, droidExtended.id);
          empireExtended = empireExtendedClient.one<EmpireExtended>(someVersion, "id", "empire");
          afterAssociateCount = empireExtended.roster.Count();
        };
        it["the derived empire should not be null"] = () => empireExtended.should_not_be_null();
        it["the derived droid should be a derived participant of the derived empire"] = () => empireExtended.roster.FirstOrDefault(d => d.party.id == "2-1B").should_not_be_null();
        it["the count of the roster after associate should always be the same or 1 more than the count of the roster before associate and the difference should never more than 1"] = () => afterAssociateCount.should(n => n <= beforeAssociateCount + 1);
      };

      describe["can disassociate a participant from an affiliation"] = () => {
        string someVersion = "v$12345678";
        before = () => {
          var droidClient = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
          var droid = new Droid { id = "2-1B", name = "2-1B", ts = DateTime.Now };
          droidClient.save(someVersion, droid);

          var empireClient = ServiceProvider.Instance.GetService<EmpireClient<Empire>>();
          var badRoster = new List<Participant> { new Participant { id = droid.id, party = droid } };
          var empire = new Empire { id = "empire", roster = badRoster, ts = DateTime.Now };
          empireClient.save(someVersion, empire);
        };
        act = () => {
          empireClient = ServiceProvider.Instance.GetService<EmpireClient<Empire>>();
          empire = empireClient.one<Empire>(someVersion, "id", "empire");
          var droidClient = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
          droid = droidClient.one<Droid>(someVersion, "id", "2-1B");
          empireClient.disassociate<Participant>(someVersion, empire.id, droid.id);
          empire = empireClient.one<Empire>(someVersion, "id", "empire");
        };
        it["the droid should no longer be a participant of the empire"] = () => empire.roster.FirstOrDefault(d => d.party.id == "2-1B").should_be_null();
      };

      describe["can pass a party type to one function for Affiliation"] = () => {
        string someVersion = "v$12345678";
        before = () => {
          var droidClient = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
          var droid = new Droid { id = "2-1B", name = "2-1B", ts = DateTime.Now };
          droidClient.save(someVersion, droid);

          var empireClient = ServiceProvider.Instance.GetService<EmpireClient<Empire>>();
          var badRoster = new List<Participant> { new Participant { id = droid.id, party = droid } };
          var empire = new Empire { id = "empire", roster = badRoster, ts = DateTime.Now };
          empireClient.save(someVersion, empire);
        };
        act = () => {
          empireClient = ServiceProvider.Instance.GetService<EmpireClient<Empire>>();
          empire = empireClient.one<Empire>(someVersion, "id", "empire", typeof(Droid));
          droid = empire.roster.FirstOrDefault(d => d.party.id == "2-1B").party as Droid;
        };
        it["the party type, droid, should be the same as the one passed into the one function"] = () => droid.id.should_be("2-1B");
      };

      describe["can assign a list Affiliation to a Model"] = () => {
        string someVersion = "v$12345678";
        int beforeAllianceCount = 0;
        int afterAllianceCount = 0;

        before = () => {
          empireClient = ServiceProvider.Instance.GetService<EmpireClient<Empire>>();
          droidClient = ServiceProvider.Instance.GetService<DroidClient<Droid>>();

          // Create an Empire
          var empire = new Empire { id = "empire", roster = { }, ts = DateTime.Now };
          empireClient.save(someVersion, empire);

          // Create a Droid
          var droid = new Droid { id = "2-1B", name = "2-1B", ts = DateTime.Now };
          // Assign an Affiliation to the droid
          droid.alliance.Add(empire);
          droidClient.save(someVersion, droid);

          // Check how many alliances the droid has
          droid = droidClient.one<Droid>(someVersion, "id", "2-1B");
          beforeAllianceCount = droid.alliance.Count();          
        };
        act = () => {
          empire = empireClient.one<Empire>(someVersion, "id", "empire");
          // Assign the same Affiliation to the droid 3 more times and get count
          foreach (int i in Enumerable.Range(1, 3)) {
            droid = droidClient.one<Droid>(someVersion, "id", "2-1B");
            // New list of Affiliation
            droid.alliance = new List<Affiliation<Participant>> { empire };
            droidClient.save(someVersion, droid);
          }
          droid = droidClient.one<Droid>(someVersion, "id", "2-1B");
          afterAllianceCount = droid.alliance.Count();
        };

        it["the counts for the list of affiliation of a droid before replacing it 3 times with the same affiliation and proves the save function is doing a replace and not a concat"] = () => afterAllianceCount.should(count => count == beforeAllianceCount);
      };

      describe["the party object is strongly typed even if it is from a derived affiliation of a derived affiliation"] = () => {
        string someVersion = "v$12345678";
        before = () => {
          //Create a Affiliation that is derived from the Empire called FirstOrder
          ServiceProvider.Instance.Singleton<Client<FirstOrder>>(() => new Client<FirstOrder>(dbContext));
          ServiceProvider.Instance.Singleton<Client<DroidExtended>>(() => new Client<DroidExtended>(dbContext));
          firstOrderClient = ServiceProvider.Instance.GetService<Client<FirstOrder>>();
          var firstOrder = new FirstOrder { id = "first-order", name = "The First Order", ts = DateTime.Now, roster = { } };

          firstOrderClient.save<FirstOrder>(someVersion, firstOrder);

          var droidExtendedClient = ServiceProvider.Instance.GetService<Client<DroidExtended>>();
          var droidExtended = new DroidExtended { id = "2-1B", name = "2-1B", ts = DateTime.Now, language = "None" };
          droidExtendedClient.save(someVersion, droidExtended);
          firstOrderClient.associate<ParticipantExtended, DroidExtended>(someVersion, "first-order", droidExtended.id);
        };
        act = () => {
          // Passing a "party" Type to an derived Affiliation and check that the party is a DroidExtended and not a dynamic type
          var firstOrderObj = firstOrderClient.one<Record<FirstOrder>>(someVersion, "id", "first-order", typeof(DroidExtended));
          dynamicObject = firstOrderObj.current.roster.FirstOrDefault(d => d.party.id == "2-1B").party;          
        };

        it["the party object should be a type of DroidExtended"] = () => dynamicObject.GetType().should(t => t == typeof(DroidExtended));
      };

      describe["can count a collection"] = () => {
        string someVersion = "v$12345678";
        long count = 0;
        before = () => {
          var droidClient = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
          var droid = new Droid { id = "2-1B", name = "2-1B", ts = DateTime.Now };
          droidClient.save(someVersion, droid);
        };

        describe["can count without a search filter"] = () => {
          act = () => {
            var droidClient = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
            count = droidClient.count(someVersion);
          };
          it["the collection count is not zero"] = () => count.should_be_greater_than(0);
        };

        describe["can count with a search filter"] = () => {
          act = () => {
            var droidClient = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
            count = droidClient.count(someVersion, "id", "2-1B");
          };
          it["the collection count should be one"] = () => count.should_be(1);
        };
      };

      describe["can search with offset and limit"] = () => {
        string someVersion = "v$12345678";
        
        before = () => {
          var droidClient = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
          var droid = new Droid { id = "2-1B", name = "2-1B", ts = DateTime.Now };
          droidClient.save(someVersion, droid);
        };

        describe["can search without limit and filter"] = () => {
          List<Record<Droid>> list = new List<Record<Droid>>();
          act = () => {
            var droidClient = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
            list = droidClient.search(someVersion, "id", "2-");
          };
          it["the collection should contain 1 record"] = () => list.Count().should_be(1);
        };

        describe["can search with a limit and filter"] = () => {
          List<Record<Droid>> list = new List<Record<Droid>>();
          before = () => {
            var droidClient = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
            var droid = new Droid { id = "R2-D2", name = "R2-D2", ts = DateTime.Now };
            droidClient.save(someVersion, droid);
          };

          act = () => {
            var droidClient = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
            list = droidClient.search(someVersion, "id", "R2", 0, 1);
          };
          it["the collection should contain 1 record because limit was set to 1"] = () => list.Count().should_be(1);
        };
      };

      describe["when creating a new collection"] = () => {
        before = () => {
          var droidClient = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
          var droid = new Droid { id = "R2-D2", name = "R2-D2", ts = DateTime.Now };
          droidClient.save(someVersion, droid);
        };

        act = () => {
          var droidClient = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
          droid = droidClient.one<Droid>(someVersion, "id", "R2-D2");
        };

        it["the name of the droid"] = () => droid.name.should_be("R2-D2");
      };

      describe["when updating a party after associating it to an affiliation"] = () => {
        ServiceProvider.Instance.Singleton<HumanClient<Human>>(() => new HumanClient<Human>(dbContext));
        ServiceProvider.Instance.Singleton<Client<RebelAlliance>>(() => new Client<RebelAlliance>(dbContext));

        var humanClient = ServiceProvider.Instance.GetService<HumanClient<Human>>();
        var rebelAllianceClient = ServiceProvider.Instance.GetService<Client<RebelAlliance>>();

        Record<RebelAlliance> currentRebelAlliance = null;
        Human currentLeia = null;

        before = () => {
          var generalLeia = new Human { id = "organa-leia", name = "Leia Organa" };
          humanClient.save(someVersion, generalLeia);

          var rebelAlliance = new RebelAlliance { id = "rebel-alliance", name = "The Rebel Alliance", ts = DateTime.Now, roster = { } };
          rebelAllianceClient.save(someVersion, rebelAlliance);
        };

        describe["can associate a party to an affiliation"] = () => {
          before = () => {
            // Get saved Leia.
            currentLeia = humanClient.one<Human>(someVersion, "id", "organa-leia");

            // Associate Leia to Rebel Alliance.
            rebelAllianceClient.associate<Participant, Human>(someVersion, "rebel-alliance", currentLeia.id);
          };

          act = () => {
            // Get the latest participants to Rebel Alliance.
            currentRebelAlliance = rebelAllianceClient.one<Record<RebelAlliance>>(someVersion, "id", "rebel-alliance", typeof(Human));
            currentLeia = currentRebelAlliance.current.roster.FirstOrDefault(d => d.id == "organa-leia").party;
          };

          it["the name of the party in the current roster"] = () => currentLeia.name.should_be("Leia Organa");

          describe["can get the latest changes to a party from a roster"] = () => {
            before = () => {
              // Get saved Leia.
              currentLeia = humanClient.one<Human>(someVersion, "id", "organa-leia");

              // Leia married Han. Update Leia's name.
              currentLeia.name = "Leia Organa Solo";
              humanClient.save(someVersion, currentLeia);
            };

            act = () => {
              // Get the latest participants to Rebel Alliance.
              currentRebelAlliance = rebelAllianceClient.one<Record<RebelAlliance>>(someVersion, "id", "rebel-alliance", typeof(Human));
              currentLeia = currentRebelAlliance.current.roster.FirstOrDefault(d => d.id == "organa-leia").party;
            };

            it["the name of the party in the current roster"] = () => currentLeia.name.should_be("Leia Organa Solo");
          };
        };        
      };
    }

    private DroidClient<Droid> droidClient;
    private DroidClient<Droid> droidClientA;
    private DroidClient<Droid> droidClientB;
    private EmpireClient<Empire> empireClient;
    private Client<FirstOrder> firstOrderClient;
    private Empire empire;
    private Droid droid;
    private EmpireExtended empireExtended;
    private Droid droidExtended;
    private List<VersionControl> versions = new List<VersionControl>();
    private List<Record<Personnel>> personnelList = new List<Record<Personnel>>();
    private VersionControl newVc = null;
    private VersionControl newVc2 = null;
    private PersonnelClient<Personnel> personnelClient;
    private Personnel personnel;
    private Record<Personnel> recordOfPersonnel;
    private object dynamicObject;
  }

  class Index {
    static void Main(string[] args) {
      
      string foo_bar_1 = "foo bar 1";
      string someVersion = "v$12345678";

      var dbContext = new DBContext { server = "127.0.0.1", port = 5432, database = "pccrms", userId = "editor", password = "editor" };
      // ----------------------------------------------------------------------------------------------------------
      // ServiceProvider.Instance.Singleton<Client<Personnel>>(() => new Client<Personnel>(dbContext));
      // var pclient = ServiceProvider.Instance.GetService<Client<Personnel>>();
      // pclient.save("v$12345678", new Personnel { id = "0", name = "abc123", ts = DateTime.Now });
      // var p = pclient.one<Personnel>("v$0", "id", "taoh02");
      // var l = pclient.list("v$0", 0, 10);

      // ----------------------------------------------------------------------------------------------------------
      ServiceProvider.Instance.Singleton<VersionControlManager>(() => new VersionControlManager(dbContext));
      // var vc = ServiceProvider.Instance.GetService<VersionControlManager>();
      // var vcs = vc.getVersionControls();
      // var vcs1 = vc.getVersionControls().Where(d => d.name == foo_bar_1).ToList();
      // vc.createNewVersionControl(foo_bar_1);

      // ----------------------------------------------------------------------------------------------------------
      // Create an Affiliation
      // ServiceProvider.Instance.Singleton<EmpireClient<Empire>>(() => new EmpireClient<Empire>(dbContext));
      // var empireClient = ServiceProvider.Instance.GetService<EmpireClient<Empire>>();
      // var badRoster = new List<Participant>();
      // var empire = new Empire { id = "empire", roster = badRoster, ts = DateTime.Now };
      // empireClient.save(someVersion, empire);
      // Associate something to that affiliation
      // ServiceProvider.Instance.Singleton<DroidClient<Droid>>(() => new DroidClient<Droid>(dbContext));
      // var droidClient = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
      // var droid = new Droid { id = "2-1B", name = "2-1B", ts = DateTime.Now };
      // droidClient.save(someVersion, droid);
      // var empireObj = empireClient.one<Record<Empire>>(someVersion, "id", "empire");
      // empireClient.associate<Participant, Droid>(someVersion, empireObj.id, droid.id);

      // ----------------------------------------------------------------------------------------------------------
      // Create an extended Affiliation
      // ServiceProvider.Instance.Singleton<Client<EmpireExtended>>(() => new Client<EmpireExtended>(dbContext));
      // var empireExtendedClient = ServiceProvider.Instance.GetService<Client<EmpireExtended>>();
      // var badRosterExtended = new List<ParticipantExtended>();
      // var empireExtended = new EmpireExtended { id = "empire", roster = badRosterExtended, ts = DateTime.Now };
      // empireExtendedClient.save(someVersion, empireExtended);
      // Associate something that was also extended to that extended affiliation
      // ServiceProvider.Instance.Singleton<DroidClient<DroidExtended>>(() => new DroidClient<DroidExtended>(dbContext));
      // var droidExtendedClient = ServiceProvider.Instance.GetService<DroidClient<DroidExtended>>();
      // var droidExtended = new DroidExtended { id = "2-1B", name = "2-1B", ts = DateTime.Now, language = "None" };
      // droidExtendedClient.save(someVersion, droidExtended);

      // var empireExtendedObj = empireExtendedClient.one<Record<EmpireExtended>>(someVersion, "id", "empire");
      // empireExtendedClient.associate<ParticipantExtended, DroidExtended>(someVersion, empireExtendedObj.id, droidExtended.id);

      // Passing a "party" Type to an Affiliation and check that the party is a DroidExtended and not a dynamic type
      // empireExtendedObj = empireExtendedClient.one<Record<EmpireExtended>>(someVersion, "id", "empire", typeof(DroidExtended));

      // Associate again 3 times and check for dupes
      // foreach(int i in Enumerable.Range(1, 3)) {
      //   empireExtendedClient.associate<ParticipantExtended, DroidExtended>(someVersion, empireExtendedObj.id, droidExtended.id);
      // }
      // empireExtendedClient.associate<ParticipantExtended, DroidExtended>(someVersion, empireExtendedObj.id, droidExtended.id);

      // Disassociate from an Affiliation
      // empireExtendedClient.disassociate<ParticipantExtended>(someVersion, empireExtendedObj.id, droidExtended.id);

      // ----------------------------------------------------------------------------------------------------------
      // Affiliation should fetch the latest party record.
      ServiceProvider.Instance.Singleton<HumanClient<Human>>(() => new HumanClient<Human>(dbContext));
      ServiceProvider.Instance.Singleton<Client<RebelAlliance>>(() => new Client<RebelAlliance>(dbContext));

      Record<RebelAlliance> currentRebelAlliance = null;
      Human currentLeia = null;

      var humanClient = ServiceProvider.Instance.GetService<HumanClient<Human>>();
      var generalLeia = new Human { id = "organa-leia", name = "Leia Organa" };
      humanClient.save(someVersion, generalLeia);

      var rebelAllianceClient = ServiceProvider.Instance.GetService<Client<RebelAlliance>>();
      var rebelAlliance = new RebelAlliance { id = "rebel-alliance", name = "The Rebel Alliance", ts = DateTime.Now, roster = { } };
      rebelAllianceClient.save(someVersion, rebelAlliance);

      // Associate Leia to Rebel Alliance.
      rebelAllianceClient.associate<Participant, Human>(someVersion, "rebel-alliance", generalLeia.id);
      
      // Get the latest participants to Rebel Alliance.
      currentRebelAlliance = rebelAllianceClient.one<Record<RebelAlliance>>(someVersion, "id", "rebel-alliance", typeof(Human));

      currentLeia = currentRebelAlliance.current.roster.FirstOrDefault(d => d.id == "organa-leia").party;
      Console.WriteLine(currentLeia.name);

      // Leia married Han. Update Leia's name.
      generalLeia.name = "Leia Organa Solo";
      humanClient.save(someVersion, generalLeia);

      // Get the latest participants to Rebel Alliance.
      currentRebelAlliance = rebelAllianceClient.one<Record<RebelAlliance>>(someVersion, "id", "rebel-alliance", typeof(Human));
      currentLeia = currentRebelAlliance.current.roster.FirstOrDefault(d => d.id == "organa-leia").party;
      // Name should now be Leia Organa Solo.
      Console.WriteLine(currentLeia.name);

      // ----------------------------------------------------------------------------------------------------------
      // Calling one when version and schema does not exist will not throw
      var fac = new Factory<NpgsqlConnection>();
      var dbConnection = fac.Get(dbContext);
      CommandRunner runner = new CommandRunner(dbConnection);
      try {
        runner.Transact(new DbCommand[] { runner.BuildCommand("drop schema v$12345678 cascade", null) });
      } catch (Exception err) {
        // Expect -> 3F000: schema "v$12345678" does not exist
        Console.WriteLine(err.Message);
      }
      ServiceProvider.Instance.Singleton<DroidClient<Droid>>(() => new DroidClient<Droid>(dbContext));
      var someDroidClient = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
      var someDroid = someDroidClient.one<Droid>(someVersion, "id", "R2-D2");
      Console.WriteLine(someDroid == null);

      // ----------------------------------------------------------------------------------------------------------
      // Calling save when version and schema does not exist will not throw
      var newDroid = new Droid { id = "2-1B", name = "2-1B", ts = DateTime.Now };
      someDroidClient.save("v$12345678", newDroid);
      var fetchNewDroid = someDroidClient.one<Droid>(someVersion, "id", "2-1B");
      Console.WriteLine(fetchNewDroid != null);

      // ----------------------------------------------------------------------------------------------------------
      //Create a Affiliation that is derived from the Empire called FirstOrder
      ServiceProvider.Instance.Singleton<Client<FirstOrder>>(() => new Client<FirstOrder>(dbContext));
      ServiceProvider.Instance.Singleton<Client<DroidExtended>>(() => new Client<DroidExtended>(dbContext));
      var firstOrderClient = ServiceProvider.Instance.GetService<Client<FirstOrder>>();
      var firstOrder = new FirstOrder { id = "first-order", name = "The First Order", ts = DateTime.Now, roster = { } };

      firstOrderClient.save<FirstOrder>(someVersion, firstOrder);

      var droidExtendedClient = ServiceProvider.Instance.GetService<Client<DroidExtended>>();
      var droidExtended = new DroidExtended { id = "2-1B", name = "2-1B", ts = DateTime.Now, language = "None" };
      droidExtendedClient.save(someVersion, droidExtended);
      firstOrderClient.associate<ParticipantExtended, DroidExtended>(someVersion, "first-order", droidExtended.id);

      // Passing a "party" Type to an derived Affiliation and check that the party is a DroidExtended and not a dynamic type
      var firstOrderObj = firstOrderClient.one<Record<FirstOrder>>(someVersion, "id", "first-order", typeof(DroidExtended));

      // ----------------------------------------------------------------------------------------------------------
      // Create List<IModel> as attribute of Model and associate it with Affiliation
      ServiceProvider.Instance.Singleton<EmpireClient<Empire>>(() => new EmpireClient<Empire>(dbContext));
      var empireClient = ServiceProvider.Instance.GetService<EmpireClient<Empire>>();
      ServiceProvider.Instance.Singleton<DroidClient<Droid>>(() => new DroidClient<Droid>(dbContext));
      var droidClient = ServiceProvider.Instance.GetService<DroidClient<Droid>>();
      
      // Create an Empire
      var empire = new Empire { id = "empire", roster = { }, ts = DateTime.Now };
      empireClient.save(someVersion, empire);

      // Get a count
      var count = empireClient.count(someVersion, "id", "empire");

      // Create a Droid
      var droid = new Droid { id = "2-1B", name = "2-1B", ts = DateTime.Now };
      // Assign an Affiliation to the droid
      droid.alliance.Add(empire);
      droidClient.save(someVersion, droid);

      // Check how many alliances the droid has
      droid = droidClient.one<Droid>(someVersion, "id", "2-1B");
      int allianceCount = 0;
      allianceCount = droid.alliance.Count();

      // Assign the same Affiliation to the droid 3 times and get count
      foreach(int i in Enumerable.Range(1, 3)) {
        droid = droidClient.one<Droid>(someVersion, "id", "2-1B");
        droid.alliance = new List<Affiliation<Participant>> { empire };
        droidClient.save(someVersion, droid);
      }
      allianceCount = droid.alliance.Count();
      // ----------------------------------------------------------------------------------------------------------

      /**
       * packages\nspec.1.0.13\tools\NSpecRunner.exe C:\cygwin64\home\htao\Store\Store.Storage.Pgsql.Test\bin\Debug\Store.Storage.Pgsql.Test.dll
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