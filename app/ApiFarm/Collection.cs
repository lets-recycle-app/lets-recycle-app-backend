using System;

namespace ApiFarm
{
    public class Collection
    {
        private ApiFarm api;

        public Collection(ApiFarm _api)
        {
            api = _api;
        }
        
        public void Confirm()
        {
            Console.WriteLine("Collection Confirm");
            
            api.ApiStatus(201, "Created");
        }

        public void Update()
        {
            Console.WriteLine("Collection Update");
            
            api.ApiStatus(202, "Updated");
        }

        public void Cancel()
        {
            Console.WriteLine("Collection Cancel");
            api.ApiStatus(203, "Deleted");
        }
    }
}