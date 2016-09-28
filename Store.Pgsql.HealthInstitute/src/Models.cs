using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Store.Models;

namespace Store.Pgsql.HealthInstitute {
  namespace Models {
    /// <summary>
    /// Member derives from Participant to include additional properties such as effectiveDate and isLeadership.
    /// </summary>
    public class Member : Participant {
      public DateTime effectiveDate { get; set; }
      public int isLeadership { get; set; }
    }

    /// <summary>
    /// Base properties for all programs:
    /// ResearchProgram, DiseaseManagementGroup, SeniorLeadership, SharedResources
    /// Note:
    /// effectiveDate used for when added by user
    /// Program cannot be deleted
    /// </summary>
    public abstract class Program : Affiliation<Member> {
      public DateTime effectiveDate { get; set; }
      public string degrees { get; set; }
    }

    /// <summary>
    /// Multi-select pick list
    /// </summary>
    public class ResearchProgram : Program {
      public string code { get; set; }
    }

    /// <summary>
    /// Multi-select pick list
    /// </summary>
    public class DiseaseManagementGroup : Program { }

    /// <summary>
    /// Multi-select pick list
    /// </summary>
    public class SeniorLeadership : Program { }

    /// <summary>
    /// Multi-select pick list
    /// </summary>
    public class SharedResources : Program {
      public List<NCICategory> nciCategory { get; set; }
      public int developingResource { get; set; }
    }

    /// <summary>
    /// Membership must implement IModel.
    /// All classes implementing IModel must have specify the static function: toStorage().
    /// Note:
    /// membershipApplication, biosketch, cv, otherAttachments are document references.
    /// The attribute class DocumentStorage is used to specific where they are kept (i.e. OnBase).
    /// The id and name attributes should be the actual values used in those storage system.
    /// otherAttachments is a collection.
    /// </summary>
    public class Personnel : Model {
      public DateTime effectiveDate { get; set; }
      public string lastName { get; set; }
      public string firstName { get; set; }
      public string middleInitial { get; set; }
      public string degree { get; set; }
      public string title { get; set; }
      public Department department { get; set; }
      public Division division { get; set; }
      public string academicTitle { get; set; }
      public string kerberosId { get; set; }
      public string eRACommonsId { get; set; }
      public string pubMedId { get; set; }
      public string email { get; set; }
      public string address { get; set; }
      public string phoneNumber { get; set; }
      public int peerReviewedFunding { get; set; }
      public MembershipType membershipType { get; set; }
      public List<ResearchProgram> researchProgram { get; set; }
      public List<DiseaseManagementGroup> diseaseManagementGroup { get; set; }
      public List<SeniorLeadership> seniorLeadership { get; set; }
      public List<SharedResources> sharedResources { get; set; }
      public MembershipStatus membershipStatus { get; set; }
      public List<Document> membershipApplication { get; set; }
      public List<Document> biosketch { get; set; }
      public List<Document> cv { get; set; }
      public List<Document> otherAttachments { get; set; }
      public string comments { get; set; }
    }

    /// <summary>
    /// Pick list
    /// </summary>
    public class Department : Model { }

    /// <summary>
    /// Pick list
    /// </summary>
    public class Division : Model { }

    /// <summary>
    /// Pick list
    /// </summary>
    public class MembershipType : Model { }

    /// <summary>
    /// NCI Defined Categories
    /// Multi-select pick list
    /// </summary>
    public class NCICategory : Model { }

    /// <summary>
    /// Pick list
    /// </summary>
    public class MembershipStatus : Model { }

    /// <summary>
    /// The id and name attributes should be the actual values used in those storage system.
    /// </summary>
    public class Document : Model {
      public DocumentStorage storage { get; set; }
    }

    /// <summary>
    /// DocumentStorage is used to specific where the documents are physically kept (i.e. OnBase, Windows file share, etc).
    /// </summary>
    public class DocumentStorage : Model { }
  }
}
