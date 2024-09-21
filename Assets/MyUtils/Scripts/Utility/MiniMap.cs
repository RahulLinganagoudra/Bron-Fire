using UnityEngine;
using UnityEngine.UI;

namespace MyUtils
{
    public class MiniMap : MonoBehaviour
    {
        enum MiniMapReferenceType
        {
            wrtNone,
            wrtPlayer,
            wrtCamera
        }
        #region references
        
        [Header("ReferenceType :")]
        [SerializeField] MiniMapReferenceType type;
        [Range(5f, 50f)] [SerializeField] float mapRange=10f;
        [Header("UI :")]
        [SerializeField] Image northSprite;
        [SerializeField] Image playerSprite;
        [SerializeField] Image selectionSprite;
        #endregion

        #region fields

        private GameObject player;
        private Camera cam;
        public  Transform selection;
        private float sqrdRange;
        private float radius = 42f;

        #endregion

        // Start is called before the first frame update
        void Start()
        {
            player = GameObject.FindWithTag("Player");
            cam=Camera.main;
            //tested value ratio
            radius = (42f / 50f) * GetComponent<RectTransform>().rect.width / 2f;
            northSprite.rectTransform.position = transform.position + Vector3.up * radius;
            sqrdRange = mapRange * mapRange;

        }
        private void Update()
        {
            //MiniMapWRTNone();
            MiniMapWRTPlayer();
        }

       

        private void CalculateDirection()
        {
            switch (type)
            {
                case MiniMapReferenceType.wrtNone:
                    MiniMapWRTNone();
                    break;
                case MiniMapReferenceType.wrtPlayer:
                    MiniMapWRTPlayer();
                    break;
                case MiniMapReferenceType.wrtCamera:
                    MiniMapWRTCamera();
                    break;
                default:
                    MiniMapWRTNone();
                    break;
            }
        }

        void MiniMapWRTNone()
        {
            

            Vector3 directionVector = selection.transform.position - player.transform.position;
            Vector3 tempPos = CalculateVector(directionVector,0f);

            selectionSprite.rectTransform.position = tempPos;

        }

        Vector3 CalculateVector(Vector3 directionVector,float theta)
        {
            Vector3 direction = directionVector.AngleToVectorXY(theta);

            Vector3 tempPos;
            if (directionVector.SquaredDistanceLessThanCheck(mapRange))
            {
                float rangeM = (directionVector.sqrMagnitude / sqrdRange) * radius;
                tempPos = transform.position + direction * rangeM;
            }
            else
            {
                tempPos = transform.position + direction * radius;
            }

            return tempPos;
        }

        void MiniMapWRTPlayer()
        {
            Vector3 directionVector = selection.transform.position - player.transform.position;
            float theta = player.transform.eulerAngles.y * Mathf.Deg2Rad;
            northSprite.rectTransform.position = transform.position + Vector3.forward.AngleToVectorXY(theta) * radius;

            selectionSprite.rectTransform.position=CalculateVector(directionVector, theta);

        }
        void MiniMapWRTCamera()
        {

        }
        public void SetTarget(Transform target)
        {
            if (target)selection = target;
        }
    }
}