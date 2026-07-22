namespace Application.Caching;

// Ένα σημείο που ορίζει πώς μοιάζει το cache key για μια IP διεύθυνση. Το IpInfoService
// (Task 1) γράφει σε αυτό το key, και το IpRefreshService (Task 2) το ακυρώνει όταν
// αλλάξει η χώρα μιας IP - αν ο καθένας έφτιαχνε το key με τον δικό του τρόπο, θα
// μπορούσαν να "ξεσυγχρονιστούν" αθόρυβα και η ακύρωση να μη βρίσκει ποτέ τίποτα.
public static class CacheKeys
{
    public static string ForIp(string address) => $"ipinfo:{address}";
}
