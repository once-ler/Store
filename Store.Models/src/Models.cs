using System;
using System.Collections.Generic;
using Store.Interfaces;

namespace Store {
  namespace Models {

    /// <summary>
    /// Should be a generalized endpoint to a persistant storage.
    /// Implementation details should not be made known.
    /// Examples:
    /// "MongoDB" -> new DBContext{ server = "127.0.0.1", port = 27017, database = "unicorns", userId = "foo", password = "bar", commandTimeout = 30000 }
    /// "PostgreSQL" -> new DBContext{ server = "127.0.0.1", port = 5432, database = "unicorns", userId = "foo", password = "bar", commandTimeout = 30000 }
    /// </summary>
    public struct DBContext {
      public string server;
      public int port;
      public string database;
      public string userId;
      public string password;
      public int commandTimeout;
    }

    /// <summary>
    /// The most basic type.  Nothing else.
    /// </summary>
    public abstract class Model : IModel {
      public Model() {}
      public string id { get; set; }
      public DateTime ts { get; set; }
      public string name { get; set; }
    }

    /// <summary>
    /// History of an IModel is added to the "history" attribute collection of Record{T} each time the user updates a record.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class History<T> : Model {
      public T source { get; set; }
    }

    /// <summary>
    /// Record wraps the current IModel and its history, basically a collection of IModel. 
    /// </summary>
    /// <typeparam name="T">Typically, a Class that derives from Model.</typeparam>
    public class Record<T> : Model {
      public Record() {
        history = new List<History<T>>();
      }
      public T current { get; set; }
      public List<History<T>> history { get; set; }
    }

    /// <summary>
    /// VersionControl identifies the entire set of IModel collections (i.e. Membership, Senior Leadership, Research Program, Disease Management Group, etc).  
    /// Think of VersionControl as a "database".
    /// There is one master VersionControl and users can create clones of the master.
    /// </summary>
    public class VersionControl : Model { }

    /// <summary>
    /// Participant wraps another Model.  The purpose of Participant is to timestamp when member was created for an Affiliation.
    /// </summary>
    public class Participant : Model {
      public Model party { get; set; }
    }

    /// <summary>
    /// Affiliation is what it says.
    /// </summary>
    public class Affiliation<T> : Model where T : Participant {
      public Affiliation() {
        roster = new List<T>();
      }
      public List<T> roster { get; set; }
    }
  }  
}
