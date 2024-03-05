using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TensorFlowLite.MoveNet {

    /*
     * To do mocap, disable animator & enable RecordTransformHierarchy
     */
    public class CharacterAnimationController : MonoBehaviour
    {
        [SerializeField]
        private MoveNetSinglePoseSample MoveNetSinglePoseSample;

        [SerializeField]
        public RectTransform view = null;

        [SerializeField]
        private GameObject[] joints = new GameObject[17];

        [SerializeField]
        private Animator Animator;

        [SerializeField]
        private TrainingController TrainingController;

        private MoveNetPose pose;

        private Vector3[] rtCorners = new Vector3[4];

        private Vector2 anchorPoint = new Vector2(0.5f, 0.1f);
        
        public void UpAnimationOutputEvent(int classLabel)
        {
            //TrainingController.AddOutput(classLabel, 1);
        }

        public void DownAnimationOutputEvent(int classLabel)
        {
            //TrainingController.AddOutput(classLabel, 0.5f);
        }

        protected void Update()
        {
            if (Animator.enabled) {
                return;
            }

            pose = MoveNetSinglePoseSample.GetPose();

            if (pose == null) {
                return;
            }

            view.GetWorldCorners(rtCorners);
            Vector3 min = rtCorners[0];
            Vector3 max = rtCorners[2];

            var connections = PoseNet.Connections;
            int len = connections.GetLength(0);

            for (int i = 0; i < joints.Length; i++) {
                Vector3 startPosition = Vector3.zero;
                int index = i * 2;
                startPosition.x = MoveNetSinglePoseSample.interpolatedCurrentPoses[index];
                startPosition.y = MoveNetSinglePoseSample.interpolatedCurrentPoses[index + 1];

                Vector3 position = MathTF.Lerp(min, max, new Vector3(startPosition.x, 1f - startPosition.y, 0));
                joints[i].transform.position = position;
            }
        }
    }

}