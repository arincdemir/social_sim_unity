using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;

namespace RosMessageTypes.Geometry
{
    public class PoseArray : Message
    {
        public const string RosMessageName = "geometry_msgs/PoseArray";

        // A header that contains frame id and time stamp
        public MHeader header;
        // An array of poses
        public MPose[] poses;

        public PoseArray()
        {
            header = new MHeader();
            poses = new MPose[0];
        }

        public PoseArray(MHeader header, MPose[] poses)
        {
            this.header = header;
            this.poses = poses;
        }

        public override List<byte[]> SerializationStatements()
        {
            var listOfSerializations = new List<byte[]>();
            listOfSerializations.AddRange(header.SerializationStatements());
            listOfSerializations.Add(BitConverter.GetBytes(poses.Length));
            foreach (var entry in poses)
                listOfSerializations.Add(entry.Serialize());
            return listOfSerializations;
        }

        public override int Deserialize(byte[] data, int offset)
        {
            offset = header.Deserialize(data, offset);
            var posesArrayLength = DeserializeLength(data, offset);
            offset += 4;
            poses = new MPose[posesArrayLength];
            for (var i = 0; i < posesArrayLength; i++)
            {
                poses[i] = new MPose();
                offset = poses[i].Deserialize(data, offset);
            }
            return offset;
        }

        public override string ToString()
        {
            return "PoseArray: \nheader: " + header.ToString() +
                   "\nposes: " + string.Join(", ", poses.Select(p => p.ToString()).ToArray());
        }
    }
}