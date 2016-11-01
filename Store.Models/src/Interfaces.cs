using System;
using System.Collections.Generic;
using Store.Models;

namespace Store {
  namespace Interfaces {
    /// <summary>
    /// All models must implement IModel.
    /// Models include Membership, Senior Leadership, Research Program, Disease Management Group, etc.
    /// </summary>
    public interface IModel {}

    /// <summary>
    /// Class that creates a new copy of a "database" from a master "database" or from an existing "database".
    /// "Database" is an notion, it is up to you to implement what a "database" is.
    /// Typically, a "database" is thought of as a container of one or more tables.
    /// </summary>
    public interface IVersionControlManager {

      /// <summary>
      /// Get a list of all VersionControl available in the store.
      /// </summary>
      /// <returns>List of VersionControl.  The master VersionControl should not be an option.</returns>
      List<VersionControl> getVersionControls();

      /// <summary>
      /// Create a new VersionControl with a friendly name provided by the user.
      /// This will in effect clone all collections from the master and thus may be time consuming.
      /// On success, a new VersionControl identifier will be provided.
      /// </summary>
      /// <param name="friendlyName">User defined name for the new VersionControl.</param>
      /// <returns>A new VersionControl object of the attempted VersionControl creation.</returns>
      /// Note: If there was an error in creating, Exception will be thrown.
      /// It is up to you to catch it.
      VersionControl createNewVersionControl(string friendlyName);

      /// <summary>
      /// Create a new VersionControl from an existing one.  This essentially creates a fork-join model where the latest changes of an existing VersionControl becomes a new branch.
      /// </summary>
      /// <param name="existingVersionId">An existing VersionControl identifier.</param>
      /// <param name="friendlyName">User defined name for the new VersionControl.</param>
      /// <returns>A new VersionControl object of the attempted VersionControl creation.</returns>
      /// Note: If there was an error in creating, Exception will be thrown.
      /// It is up to you to catch it.
      VersionControl createNewVersionControlFromExisting(string existingVersionId, string friendlyName);
    }
    
    /// <summary>
    /// All models are kept in stores.
    /// All stores must implement IStore.
    /// Each IStore must provide the same API to save/list/archive.
    /// Note:
    /// Although each model implements their own getVersionControls() and createNewVersionControl() functions for VersionControl,
    /// versions are shared amongst all models.
    /// For example, IStore{Membership} and IStore{Grants} share the same access to VersionControl.
    /// </summary>
    /// <typeparam name="T">A type that implements IModel.</typeparam>
    public interface IStore<T> where T : class, IModel {
      
      /// <summary>
      /// Fetch a limited number of IModel records for a VersionControl.
      /// Typically, this array will be serialized as a JSON array and sent to a client in a web application.
      /// </summary>
      /// <param name="version">The VersionControl identifier for this IModel.</param>
      /// <param name="offset">The number to start the search.</param>
      /// <param name="limit">The total number of records to search.</param>
      /// <returns>List of IModel records.</returns>
      List<Record<T>> list(string version, int offset, int limit);

      /// <summary>
      /// Provided a run-time dynamic object, convert it to a statically typed Record of an IModel.
      /// Note:
      /// This is typically used internally by the IStore implementation when data is fetched from the store and must be deserialized into a statically typed object.
      /// </summary>
      /// <typeparam name="U">U should be a type that derives from Model.</typeparam>
      /// <param name="d">The dynamic object.</param>
      /// <returns>One record of IModel.</returns>
      Record<T> makeRecord(dynamic d);

      /// <summary>
      /// Provided a JSON object string, convert it to a statically typed Record of an IModel.
      /// Note:
      /// This is typically used by a web applcation when it recieves a POST request from a client.
      /// </summary>
      /// <typeparam name="U">U should be a type that derives from Model.</typeparam>
      /// <param name="jsonString">A valid JSON object string.</param>
      /// <returns>One record of IModel.</returns>
      Record<T> makeRecord(string jsonString);

      // <summary>
      /// Create a new IModel by combining an existing IModel with another IModel.
      /// The source IModel may only contain a subset of attributes.
      /// If the attributes do not appear in the source, naturally the destination IModel will keep the existing values for those missing attributes.
      /// </summary>
      /// <param name="dest">The IModel to merge into.</param>
      /// <param name="source">The IModel that may contain only subset of attributes.</param>
      /// <returns>A new IModel with any updated attribute values from the source.</returns>
      T merge(T dest, T source);

      /// <summary>
      /// Fetch one Record{IModel} record with the provided key/value for a VersionControl.
      /// Typically, this statically typed object will be serialized as a JSON object and sent to a client in a web application. 
      /// </summary>
      /// <typeparam name="U">IModel or Record{IModel}</typeparam>
      /// <param name="version">The VersionControl identifier for this IModel.</param>
      /// <param name="field">The key field to search.  If nested, use dot notation.</param>
      /// <param name="value">The value for the key field to search.</param>
      /// <param name="typeOfParty">Provided a type, the "party" attribute of a Participant will be attempted to convert to that type.</param>
      /// <returns>One record of IModel or just IModel if exists.  If no IModel exists, error will be thrown.</returns>
      U one<U>(string version, string field, string value, Type typeOfParty = null) where U : class;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="version"></param>
      /// <param name="typeOfStore"></param>
      /// <param name="field"></param>
      /// <param name="value"></param>
      /// <returns></returns>
      object one(string version, string typeOfStore, string field, string value);

      /// <summary>
      /// The user elects to replace the current record with one found in the history collection.
      /// This is possible because we save a copy of IModel each time a user saves changes.
      /// </summary>
      /// <param name="version">The VersionControl identifier for this IModel.</param>
      /// <param name="recordId">The record identifier of Record<IModel> that the user wants to update.</param>
      /// <param name="historyId">The identifier of a historical IModel that the user wants to use as the current one.</param>
      /// <returns>Record{IModel} of the attempted replacement of the current IModel with one an historical one.</returns>
      /// Note: If there was an error in creating, Exception will be thrown.
      /// It is up to you to catch it.
      Record<T> replaceFromHistory(string version, string recordId, string historyId);

      /// <summary>
      /// Persist the IModel to the store for a VerionControl.
      /// This typically occurs when user saves on the client in a web application.
      /// The IStore will clone the current record and deque it in the history attribute.
      /// Therefore, there will always be a copy of all changes made.
      /// The user may elect to replace the current IModel with one found in the history collection at any time. 
      /// </summary>
      /// <typeparam name="U">IModel or Record{IModel}</typeparam>
      /// <param name="version">The VersionControl identifier for this IModel.</param>
      /// <param name="doc">Record{T} where T is type of Personnel, ResearchProgram, etc.</param>
      /// <returns>The same Record{IModel} if successful.  Exception will be thrown is failure.</returns>
      /// Note: If there was an error in creating, Exception will be thrown.
      /// It is up to you to catch it.
      U save<U>(string version, U doc) where U : class;
      
      /// <summary>
      /// Fetch IModel records for a VersionControl that meets the search criteria.
      /// Note:
      /// 1. By default, only 10 IModel records will be returned.
      /// 2. By default, POSIX regex insensitive matching will be used.
      /// </summary>
      /// <param name="version">The VersionControl identifier for this IModel.</param>
      /// <param name="field">The key field to search.  If nested, use dot notation.</param>
      /// <param name="search">The value for the key field to search.</param>
      /// <returns>Collection of Record<IModel> if successful.  Empty collection if failure.</returns>
      List<Record<T>> search(string version, string field, string search);

      /// <summary>
      /// Count IModel records for a VersionControl that meets the search criteria.
      /// </summary>
      /// <param name="version">The VersionControl identifier for this IModel.</param>
      /// <param name="field">The key field to search.  If nested, use dot notation.</param>
      /// <param name="search">The value for the key field to search.</param>
      /// <returns>Count of records matching search criteria.</returns>
      long count(string version, string field = null, string search = null);

      /// <summary>
      /// Associates a Model to an Affiliation object.
      /// The method does not add the Model directly.  The Model must be wrapped as the "party" attribute of a Participant type.
      /// The Participant type is then added to the "roster" attribute of the Affiliation.
      /// The Participant type of the Affiliation may be derived so that one can create additional attributes like "isLeader", and so forth.
      /// </summary>
      /// <typeparam name="U">Participant or derived Participant with additional fields.</typeparam>
      /// <typeparam name="M">Represents the type of the "party" attribute.</typeparam>
      /// <param name="version">The VersionControl identifier for this IModel.</param>
      /// <param name="recordId">The id of the Record type for an Affiliation.  An Affiliation can also be derived.  i.e. RebelAlliance, Empire, TradeFederation, Resistance, etc.</param>
      /// <param name="partyId">The id that uniquely identifies the Model referenced in the "party" attribute of the Participant.</param>
      /// <returns></returns>
      Record<Affiliation<U>> associate<U, M>(string version, string recordId, string partyId) where U : Participant, new() where M : Model;

      /// <summary>
      /// Disassociates a Model from an Affiliation object.
      /// </summary>
      /// <typeparam name="U">Participant or derived Participant with additional fields.</typeparam>
      /// <param name="version">The VersionControl identifier for this IModel.</param>
      /// <param name="recordId">The id of the Record type for an Affiliation.  An Affiliation can also be derived.  i.e. RebelAlliance, Empire, TradeFederation, Resistance, etc.</param>
      /// <param name="partyId">The id that uniquely identifies the Model referenced in the "party" attribute of the Participant.</param>
      /// <returns></returns>
      Record<Affiliation<U>> disassociate<U>(string version, string recordId, string partyId) where U : Participant;
    }
    
  }  
}
