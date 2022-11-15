using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TOAHawk
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Team> _miLOEventTeams = new List<Team>();
        Dictionary<Team, List<Match>> _miLOMatchbyTeam = new Dictionary<Team, List<Match>>();
        Dictionary<Team, List<MatchDetail>> _miLOMatchDetailbyTeam = new Dictionary<Team, List<MatchDetail>>();
        ObservableCollection<TeamStats> _miLOTeamStats = new ObservableCollection<TeamStats>();
        const int API_TIMEOUT = 6000; // For somereason www.theorangealliance.org/api has a throttle on requests
        const int FLT_CNT_MAX = 5; // if API faults retry x times
        public MainWindow()
        {
            InitializeComponent();
            HttpClient httpClient = new HttpClient();
            RestClient rc = new RestClient("https://theorangealliance.org",httpClient);

            Task<string> response = rc.SendAsync(HttpMethod.Get, "/api/event/2223-FIM-MHFQ1/teams", "");

            if(response.Result != null )
            {
                _miLOEventTeams = JsonSerializer.Deserialize<List<Team>>(response.Result);
            }
            if(_miLOEventTeams != null)
            {
                foreach (Team t in _miLOEventTeams)
                {
                    response = rc.SendAsync(HttpMethod.Get, "/api/team/" + t.team_key + "/matches/2223", "");
                    int fltcnt = 0;
                    while (response.IsFaulted && fltcnt++ < FLT_CNT_MAX)
                    {
                        Thread.Sleep(API_TIMEOUT);
                        response = rc.SendAsync(HttpMethod.Get, "/api/team/" + t.team_key + "/matches/2223", "");
                    }
                    if (response.Result != null)
                    {
                        List<Match> tmpMatches = JsonSerializer.Deserialize<List<Match>>(response.Result);
                        if(tmpMatches != null && tmpMatches.Count>0)
                        {
                            _miLOMatchbyTeam[t] = tmpMatches;
                        }

                    }
                }

                foreach(Team t in _miLOMatchbyTeam.Keys)
                {
                    List<MatchDetail> tmpMatchDetail = new List<MatchDetail>();
                    foreach(Match m in _miLOMatchbyTeam[t].ToList())
                    {
                        response = rc.SendAsync(HttpMethod.Get, "/api/match/" + m.match_key + "/details","");
                        int fltcnt = 0;
                        while (response.IsFaulted && fltcnt++<FLT_CNT_MAX)
                        {
                            Thread.Sleep(API_TIMEOUT);
                            response = rc.SendAsync(HttpMethod.Get, "/api/match/" + m.match_key + "/details", "");
                        }

                        if (response.Result != null)
                        {
                            List<MatchDetail> md = JsonSerializer.Deserialize<List<MatchDetail>>(response.Result);
                            if(md != null && md.Count>0)
                            {
                                tmpMatchDetail.Add(md[0]);
                                if (m.station / 10 > 1)
                                {
                                    // must be blue
                                    string wlt = "W";
                                    if (md[0].red.total_points > md[0].blue.total_points)
                                    {
                                        wlt = "L";
                                    }
                                    else if (md[0].red.total_points == md[0].blue.total_points)
                                    {
                                        wlt = "T";
                                    }
                                    if(m.station %2 == 0)
                                    {
                                        _miLOTeamStats.Add(new TeamStats(
                                            t.team_number,
                                            m.match_key,
                                            m.station,
                                            wlt,
                                            md[0].blue.auto_points,
                                            md[0].blue.signal_bonus_points,
                                            md[0].blue.init_signal_sleeve_2,
                                            md[0].blue.auto_robot_2,
                                            md[0].blue.auto_junction_cone_points,
                                            md[0].blue.ownership_points,
                                            md[0].blue.end_points,
                                            md[0].blue.circuit_points,
                                            md[0].blue.beacons,
                                            md[0].blue.total_points,
                                            md[0].red.total_points,
                                            md[0].red.penalty_points_comitted,
                                            md[0].blue.penalty_points_comitted));
                                    }
                                    else
                                    {
                                        _miLOTeamStats.Add(new TeamStats(
                                            t.team_number,
                                            m.match_key,
                                            m.station,
                                            wlt,
                                            md[0].blue.auto_points,
                                            md[0].blue.signal_bonus_points,
                                            md[0].blue.init_signal_sleeve_1,
                                            md[0].blue.auto_robot_1,
                                            md[0].blue.auto_junction_cone_points,
                                            md[0].blue.ownership_points,
                                            md[0].blue.end_points,
                                            md[0].blue.circuit_points,
                                            md[0].blue.beacons,
                                            md[0].blue.total_points,
                                            md[0].red.total_points,
                                            md[0].red.penalty_points_comitted,
                                            md[0].blue.penalty_points_comitted));

                                    }

                                }
                                else
                                {
                                    // must be red
                                    string wlt = "W";
                                    if (md[0].red.total_points < md[0].blue.total_points)
                                    {
                                        wlt = "L";
                                    }
                                    else if (md[0].red.total_points == md[0].blue.total_points)
                                    {
                                        wlt = "T";
                                    }
                                    if (m.station % 2 == 0)
                                    {
                                        _miLOTeamStats.Add(new TeamStats(
                                            t.team_number,
                                            m.match_key,
                                            m.station,
                                            wlt,
                                            md[0].red.auto_points,
                                            md[0].red.signal_bonus_points,
                                            md[0].red.init_signal_sleeve_2,
                                            md[0].red.auto_robot_2,
                                            md[0].red.auto_junction_cone_points,
                                            md[0].red.ownership_points,
                                            md[0].red.end_points,
                                            md[0].red.circuit_points,
                                            md[0].red.beacons,
                                            md[0].red.total_points,
                                            md[0].blue.total_points,
                                            md[0].blue.penalty_points_comitted,
                                            md[0].red.penalty_points_comitted));
                                    }
                                    else
                                    {
                                        _miLOTeamStats.Add(new TeamStats(
                                            t.team_number,
                                            m.match_key,
                                            m.station,
                                            wlt,
                                            md[0].red.auto_points,
                                            md[0].red.signal_bonus_points,
                                            md[0].red.init_signal_sleeve_1,
                                            md[0].red.auto_robot_1,
                                            md[0].red.auto_junction_cone_points,
                                            md[0].red.ownership_points,
                                            md[0].red.end_points,
                                            md[0].red.circuit_points,
                                            md[0].red.beacons,
                                            md[0].red.total_points,
                                            md[0].blue.total_points,
                                            md[0].blue.penalty_points_comitted,
                                            md[0].red.penalty_points_comitted));

                                    }
                                }

                            }
                            Thread.Sleep(API_TIMEOUT);
                        }
                    }
                    _miLOMatchDetailbyTeam[t] = tmpMatchDetail;
                }
            }

            WLTGrid.ItemsSource = _miLOTeamStats;
        }


    }

    public class TeamDetail
    {
        public string team_key { get; set; }
        public string region_key { get; set; }
        public string league_key { get; set; }
        public int team_number { get; set; }
        public string team_name_short { get; set; }
        public string team_name_long { get; set; }
        public string robot_name { get; set; }
        public string last_active { get; set; }
        public string city { get; set; }
        public string state_prov { get; set; }
        public string zip_code { get; set; }
        public string country { get; set; }
        public int rookie_year { get; set; }
        public string website { get; set; }
    }

    public class Team
    {
        public string event_participant_key { get; set; }
        public string event_key { get; set; }
        public string team_key { get; set; }
        public int team_number { get; set; }
        public bool is_active { get; set; }
        public string card_status { get; set; }
        public TeamDetail team { get; set; }
    }

    public class Match
    {
        public string match_participant_key { get; set; }
        public string match_key { get; set; }
        public string team_key { get; set; }
        public int station { get; set; }
        public int station_status { get; set; }
        public int ref_status { get; set; }
        public TeamDetail team { get; set; }
    }

    public class Red
    {
        public bool init_signal_sleeve_1 { get; set; }
        public bool init_signal_sleeve_2 { get; set; }
        public int signal_bonus_points { get; set; }
        public string auto_robot_1 { get; set; }
        public string auto_robot_2 { get; set; }
        public int auto_nav_points { get; set; }
        public int auto_terminal { get; set; }
        public int auto_terminal_cone_points { get; set; }
        public List<List<List<string>>> auto_junctions { get; set; }
        public List<int> auto_junction_cones { get; set; }
        public int auto_junction_cone_points { get; set; }
        public List<List<List<string>>> tele_junctions { get; set; }
        public List<int> tele_junction_cones { get; set; }
        public int tele_junction_cone_points { get; set; }
        public int tele_terminal_near { get; set; }
        public int tele_terminal_far { get; set; }
        public object tele_terminal_other { get; set; }
        public int tele_terminal_cone_points { get; set; }
        public bool end_navigated_1 { get; set; }
        public bool end_navigated_2 { get; set; }
        public int end_nav_points { get; set; }
        public int beacons { get; set; }
        public int owned_junctions { get; set; }
        public int ownership_points { get; set; }
        public bool circuit_exists { get; set; }
        public int circuit_points { get; set; }
        public string side_of_field { get; set; }
        public int team { get; set; }
        public int auto_points { get; set; }
        public int tele_points { get; set; }
        public int end_points { get; set; }
        public int penalty_points_comitted { get; set; }
        public int pre_penalty_total_points { get; set; }
        public int total_points { get; set; }
    }

    public class Blue
    {
        public bool init_signal_sleeve_1 { get; set; }
        public bool init_signal_sleeve_2 { get; set; }
        public int signal_bonus_points { get; set; }
        public string auto_robot_1 { get; set; }
        public string auto_robot_2 { get; set; }
        public int auto_nav_points { get; set; }
        public int auto_terminal { get; set; }
        public int auto_terminal_cone_points { get; set; }
        public List<List<List<string>>> auto_junctions { get; set; }
        public List<int> auto_junction_cones { get; set; }
        public int auto_junction_cone_points { get; set; }
        public List<List<List<string>>> tele_junctions { get; set; }
        public List<int> tele_junction_cones { get; set; }
        public int tele_junction_cone_points { get; set; }
        public int tele_terminal_near { get; set; }
        public int tele_terminal_far { get; set; }
        public object tele_terminal_other { get; set; }
        public int tele_terminal_cone_points { get; set; }
        public bool end_navigated_1 { get; set; }
        public bool end_navigated_2 { get; set; }
        public int end_nav_points { get; set; }
        public int beacons { get; set; }
        public int owned_junctions { get; set; }
        public int ownership_points { get; set; }
        public bool circuit_exists { get; set; }
        public int circuit_points { get; set; }
        public string side_of_field { get; set; }
        public int team { get; set; }
        public int auto_points { get; set; }
        public int tele_points { get; set; }
        public int end_points { get; set; }
        public int penalty_points_comitted { get; set; }
        public int pre_penalty_total_points { get; set; }
        public int total_points { get; set; }
    }

    public class MatchDetail
    {
        public string match_detail_key { get; set; }
        public string match_key { get; set; }
        public int red_min_pen { get; set; }
        public int blue_min_pen { get; set; }
        public int red_maj_pen { get; set; }
        public int blue_maj_pen { get; set; }
        public Red red { get; set; }
        public Blue blue { get; set; }
    }

    public class TeamStats
    {
        int team_num;
        string match_key;
        int station;
        string wlt;
        int auton_points;
        int signal_points;
        bool customSignal;
        string autoPark;
        int auto_cone_points;
        int owner_points;
        int end_points;
        int circuit_points;
        int beacons;
        int total_for_points;
        int total_against_points;
        int total_foul_points_for;
        int total_foul_points_against;

        public TeamStats(int team_id, string matchKey, int station_id, string winLossTie, int auton, int signalBonus, bool custSignal, string auto_park,int auto_cone,int ownerPts,int end, int circuit, int beaconsCnt, int totalFor, int totalAgainst,int totalFoulFor,int totalFoulAgainst)
        {
            team_num = team_id;
            match_key = matchKey;
            station = station_id;
            wlt = winLossTie;
            auton_points = auton;
            signal_points = signalBonus;
            customSignal = custSignal;
            autoPark = auto_park;
            auto_cone_points = auto_cone;
            owner_points = ownerPts;
            end_points = end;
            circuit_points = circuit;
            beacons = beaconsCnt;
            total_for_points = totalFor;
            total_against_points = totalAgainst;
            total_foul_points_for = totalFoulFor;
            total_foul_points_against = totalFoulAgainst;
        }

        public int TeamNumber
        {
            get { return team_num; }
        }

        public String Station
        {
            get
            {

                if (station == 11)
                {
                    return "Red 1";
                }
                else if (station == 12)
                {
                    return "Red 2";
                }
                else if (station == 13)
                {
                    return "Red 3";
                }
                else if (station == 21)
                {
                    return "Blue 1";
                }
                else if (station == 22)
                {
                    return "Blue 2";
                }
                else if (station == 23)
                {
                    return "Blue 3";
                }
                else
                {
                    return "NA";
                }
            }
        }
        public String MatchKey { get { return match_key; } }

        public String WLT { get { return wlt; } }
        public int AutonPoints { get { return auton_points; } }
        public int SignalPoints { get { return signal_points; } }
        public bool CustomSignal { get { return customSignal; } }
        public String AutoPark { get { return autoPark; } }
        public int AutoConePoints { get { return auto_cone_points; } }
        public int OwnedPoints { get { return owner_points; } }
        public int EndPoints { get { return end_points; } }
        public int CircuitPoints { get { return circuit_points; } }
        public int Beacons { get { return beacons; } }
        public int TotalPointsFor { get { return total_for_points; } }
        public int TotalPointsAgainst { get { return total_against_points; } }
        public int TotalFoulPointsFor { get { return total_foul_points_for; } }
        public int TotalFoulPointsAgainst { get { return total_foul_points_against; } }
    }
}
