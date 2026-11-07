using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace WingsEmu.Plugins.BasicImplementation.NosMall
{
    /// <summary>
    /// In-memory wishlist storage for NosMall.
    /// Note: Wishlists are not persisted across server restarts.
    /// For persistence, add NosMallWishlist property to CharacterDTO.
    /// </summary>
    public interface INosMallWishlistManager
    {
        bool AddToWishlist(long characterId, int itemVnum);
        bool RemoveFromWishlist(long characterId, int itemVnum);
        List<int> GetWishlist(long characterId);
        void ClearWishlist(long characterId);
        bool IsInWishlist(long characterId, int itemVnum);
    }

    public class NosMallWishlistManager : INosMallWishlistManager
    {
        private const int MaxWishlistItems = 10;
        private readonly ConcurrentDictionary<long, HashSet<int>> _wishlists = new();

        public bool AddToWishlist(long characterId, int itemVnum)
        {
            var wishlist = _wishlists.GetOrAdd(characterId, _ => new HashSet<int>());
            
            lock (wishlist)
            {
                if (wishlist.Count >= MaxWishlistItems)
                    return false;
                    
                return wishlist.Add(itemVnum);
            }
        }

        public bool RemoveFromWishlist(long characterId, int itemVnum)
        {
            if (!_wishlists.TryGetValue(characterId, out var wishlist))
                return false;
                
            lock (wishlist)
            {
                return wishlist.Remove(itemVnum);
            }
        }

        public List<int> GetWishlist(long characterId)
        {
            if (!_wishlists.TryGetValue(characterId, out var wishlist))
                return new List<int>();
                
            lock (wishlist)
            {
                return wishlist.ToList();
            }
        }

        public void ClearWishlist(long characterId)
        {
            _wishlists.TryRemove(characterId, out _);
        }

        public bool IsInWishlist(long characterId, int itemVnum)
        {
            if (!_wishlists.TryGetValue(characterId, out var wishlist))
                return false;
                
            lock (wishlist)
            {
                return wishlist.Contains(itemVnum);
            }
        }
    }
}
