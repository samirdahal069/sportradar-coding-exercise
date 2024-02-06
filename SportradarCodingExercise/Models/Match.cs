using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Match
    {
        public Match()
        {
            // Generate a new GUID for MatchId when a Match is created
            MatchId = Guid.NewGuid();
            
            //Set startTime when a Match is created
            StartTime = DateTime.Now; 
        }
        [Required]
        public Guid MatchId { get; set; }

        [Required]
        public string HomeTeam { get; set; }

        [Required]
        public string AwayTeam { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "HomeTeamScore must be a non-negative integer.")]
        public int HomeTeamScore { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "AwayTeamScore must be a non-negative integer.")]

        public int AwayTeamScore { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
