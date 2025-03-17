// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;

namespace SEAN.Scenario.PedestrianBehavior
{
    public class LeftRightRandom : Base
    {
        Agents.LeftRightRandomAgentManager agentManager;
        GameObject random;

        public override string scenario_name
        {
            get
            {
                return "LeftRightRandom";
            }
        }

        public void Start()
        {
            base.Start();
            foreach (Transform transform in pedestrianControl.transform)
            {
                if (transform.name == "LeftRightRandom")
                {
                    random = transform.gameObject;
                    break;
                }
            }
            if (random == null)
            {
                throw new System.Exception("Could not find leftrightrandom game object in pedestrian controllers");
            }
            random.SetActive(true);
            agentManager = (Agents.LeftRightRandomAgentManager)Agents.BaseAgentManager.instance;
            agentManager.Restart();
        }

        public override Trajectory.TrackedGroup[] groups
        {
            get
            {
                if (agentManager == null)
                {
                    return new Trajectory.TrackedGroup[0];
                }
                return agentManager.groups.ToArray();
            }
        }

        public override Trajectory.TrackedAgent[] agents
        {
            get
            {
                if (agentManager == null)
                {
                    return new Trajectory.TrackedAgent[0];
                }
                return agentManager.agents.ToArray();
            }
        }
    }
}