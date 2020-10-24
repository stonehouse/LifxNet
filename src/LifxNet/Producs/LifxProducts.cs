using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace LifxNet.Producs
{
    [DataContract]
    public struct LifxVendor
    {
        [DataMember]
        public int vid;
        [DataMember]
        public string name;
        [DataMember]
        public LifxProduct[] products;

        public static LifxVendor[] Load()
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(LifxVendor[]));

                var file = File.OpenRead(AppContext.BaseDirectory + "/products.json");
                var vendors = serializer.ReadObject(file);
                return vendors as LifxVendor[];
            }
            
        }
    }

    [DataContract]
    public struct LifxProduct
    {
        [DataMember]
        public int pid;
        [DataMember]
        public string name;
        [DataMember]
        public ProductFeatures features;

    }

    [DataContract]
    public struct ProductFeatures
    {
        [DataMember]
        public bool color;
        [DataMember]
        public bool chain;
        [DataMember]
        public bool matrix;
        [DataMember]
        public bool infrared;
        [DataMember]
        public bool multizone;
        [DataMember]
        public int[] temperature_range;
        [DataMember]
        public int[]? min_ext_mz_firmware_components;
    }
}
