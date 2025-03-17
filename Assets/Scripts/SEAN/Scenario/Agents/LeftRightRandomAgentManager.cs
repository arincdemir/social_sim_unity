// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System.Collections.Generic;
using UnityEngine;

namespace SEAN.Scenario.Agents
{
    public class LeftRightRandomAgentManager : BaseAgentManager
    {

        public List<IVI.INavigable> agents;
        public List<Trajectory.TrackedGroup> groups;

        public GameObject agentPrefab;

        public int pedCount;
        public GameObject LeftBoxTopLeft;
        public GameObject LeftBoxBottomRight;
        public GameObject RightBoxTopLeft;
        public GameObject RightBoxBottomRight;

        private List<bool> movingTowardsRight = new List<bool>();


        private GameObject agentsGO;

        public string scenario_name
        {
            get
            {
                return "Random";
            }
        }

        void Update()
        {
            /* arinc edit
            foreach (var agent in agents)
            {
                if (agent.CloseEnough())
                {
                    // Set the next goal
                    agent.InitDest(Util.Navmesh.RandomPose().position);
                }
            }
            */

            for (int i = 0; i < agents.Count; i++) {
                var agent = agents[i];
                
                if (agent.CloseEnough()) 
                {
                    if (movingTowardsRight[i])
                    {
                        // get random point from left box
                        Pose pose = new Pose(
                            new Vector3(
                                Random.Range(LeftBoxTopLeft.transform.position.x, LeftBoxBottomRight.transform.position.x),
                                0,
                                Random.Range(LeftBoxBottomRight.transform.position.z, LeftBoxTopLeft.transform.position.z)
                            ),
                            Quaternion.identity
                        );
                        agent.InitDest(pose.position);
                    }
                    else 
                    {   
                        // get random point from right box
                        Pose pose = new Pose(
                            new Vector3(
                                Random.Range(RightBoxTopLeft.transform.position.x, RightBoxBottomRight.transform.position.x),
                                0,
                                Random.Range(RightBoxBottomRight.transform.position.z, RightBoxTopLeft.transform.position.z)
                            ),
                            Quaternion.identity
                        );
                        agent.InitDest(pose.position);
                    }
                    movingTowardsRight[i] = !movingTowardsRight[i];
                }
                
            }
            // arinc edit end
        }

        public void Restart()
        {
            // get agents game object
            foreach (Transform transform in transform)
            {
                if (transform.gameObject.name == "Agents")
                {
                    agentsGO = transform.gameObject;
                }
            }
            Clear();

            //arinc edit

            /*
            for (int i = 0; i < numberOfAgents; i++)
            {
                SpawnAgent("Agent_" + i, Util.Navmesh.RandomPose());
            }
            */
            for (int i = 0; i < pedCount; i++){
                // pick pose inside left or right boxes.
                // first pick which box
                bool isLeft = Random.Range(0, 2) == 0;
                if (isLeft) {
                Pose pose = new Pose(
                    new Vector3(
                        Random.Range(LeftBoxTopLeft.transform.position.x, LeftBoxBottomRight.transform.position.x),
                        0,
                        Random.Range(LeftBoxBottomRight.transform.position.z, LeftBoxTopLeft.transform.position.z)
                    ),
                    Quaternion.identity
                );
                Pose dest = new Pose(
                    new Vector3(
                        Random.Range(RightBoxTopLeft.transform.position.x, RightBoxBottomRight.transform.position.x),
                        0,
                        Random.Range(RightBoxBottomRight.transform.position.z, RightBoxTopLeft.transform.position.z)
                    ),
                    Quaternion.identity
                );
                movingTowardsRight.Add(false);
                SpawnAgent("Agent_" + i, pose, dest.position);
                } else {
                Pose pose = new Pose(
                    new Vector3(
                        Random.Range(RightBoxTopLeft.transform.position.x, RightBoxBottomRight.transform.position.x),
                        0,
                        Random.Range(RightBoxBottomRight.transform.position.z, RightBoxTopLeft.transform.position.z)
                    ),
                    Quaternion.identity
                );
                Pose dest = new Pose(
                    new Vector3(
                        Random.Range(LeftBoxTopLeft.transform.position.x, LeftBoxBottomRight.transform.position.x),
                        0,
                        Random.Range(LeftBoxBottomRight.transform.position.z, LeftBoxTopLeft.transform.position.z)
                    ),
                    Quaternion.identity
                );
                movingTowardsRight.Add(true);
                SpawnAgent("Agent_" + i, pose, dest.position);
                }
            }
            // arinc edit end
        }

        void Clear()
        {
            if (!agentsGO) { return; }
            agents = new List<IVI.INavigable>();
            groups = new List<Trajectory.TrackedGroup>();
            foreach (Transform child in agentsGO.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        IVI.INavigable SpawnAgent(string name, Pose pose, Vector3 dest)
        {
            var sfRandom = Instantiate(agentPrefab, Vector3.zero, Quaternion.identity);
            IVI.INavigable agent = sfRandom.GetComponentInChildren<IVI.INavigable>();
            agent.name = name;
            agent.transform.position = pose.position;
            agent.transform.rotation = pose.rotation;
            agent.transform.parent = agentsGO.transform;
            agents.Add(agent);
            //arinc edit
            //Vector3 pos = Util.Navmesh.RandomPose().position;
            //print(name + ": " + pos);
            //agent.InitDest(pos);
            agent.InitDest(dest);
            //arinc edit
            return agent;
        }
    }
}
