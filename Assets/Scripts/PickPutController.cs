// מחלקה זו מטפלת בהרמת והנחת חפצים ע"י הדמות הראשית

using UnityEngine;
using UnityEngine.AI;

public class PickPutController : MonoBehaviour
{
    public float minDistanceToPick; // המרחק המינימלי של הדמות הראשית מאוביקט כדי שהיא תוכל להרימו
    public Color GizmosColor = Color.yellow; // צבע של הגיזמו לבדיקת מרחק מהדמות הראשית
    public Transform holdPoint; // נקודת ההרמה ביד של הדמות הראשית
    public GameObject[] PickableObjects; // אוביקטים הניתנים להרמה
    GameObject ObjPicked; // אוביקט שהדמות הראשית מחזיקה
    Vector3 objPosition; // המיקום של אוביקט ביחס לדמות הראשית לפני ההרמה כדי לדעת באיזה גובה להניחו 
    Quaternion objRotation; // הזווית של אוביקט ביחס לדמות הראשית לפני ההרמה כדי לדעת באיזו זווית להניחו

    // בדיקה באמצעות הגיזמו, של המרחק מהדמות הראשית, בו יכולה להתקיים אינטראקציה
    void OnDrawGizmosSelected()
    {
        Gizmos.color = GizmosColor;
        Gizmos.DrawWireSphere(transform.position, minDistanceToPick);
    }

    // המוגדר בתוך האנימציה, באותו הפרם, הן להרמה והן להנחה ,event הפונקציה מופעלת על ידי
    public void AnimationEvent()
    {
        if (!ObjPicked) // אם הדמות הראשית לא מחזיקה אוביקט
        {
            // בדיקה מיהו האוביקט הניתן להרמה הקרוב ביותר לדמות הראשית
            // ההשוואה מתחילה מהמרחק המינימלי להרמה + 1 כי יתכן שאף אוביקט לא מספיק קרוב
            float minDistance = minDistanceToPick + 1f;
            float tmpDistance;
            GameObject ObjTopick = null; // אוביקט שהדמות הראשית צריכה להרים
            for (int i = 0; i < PickableObjects.Length; i++)
            {
                tmpDistance = Vector3.Distance(transform.position, PickableObjects[i].transform.position);
                if (minDistance > tmpDistance)
                {
                    minDistance = tmpDistance;
                    if (minDistance <= minDistanceToPick) // אם האוביקט בטווח ההרמה
                    {
                        // ניתן להוסיף פה תנאי אם שהשחקן גם מסתכל על האוביקט
                        ObjTopick = PickableObjects[i];
                    }
                }
            }
            // אם יש אוביקט להרמה הרמת האוביקט
            if (ObjTopick)
            {
                ObjPicked = ObjTopick; // האוביקט שהדמות הראשית מרימה הוא האוביקט שעליה להרים

                // הוא יבוטל כדי שהאוביקט יהיה ניתן להרמה NavMeshObstacle אם יש לאוביקט
                if (ObjPicked.GetComponent<NavMeshObstacle>())
                    ObjPicked.GetComponent<NavMeshObstacle>().enabled = false;

                // הוא תבוטל כדי שהאוביקט לא יזוז ביד של הדמות הראשית Animator אם יש לאוביקט
                if (ObjPicked.GetComponent<Animator>())
                    ObjPicked.GetComponent<Animator>().enabled = false;

                // כוח המשיכה וזיהוי התנגשות יבוטלו ותופעל קינמטיקה Rigidbody אם יש לאוביקט 
                if (ObjPicked.GetComponent<Rigidbody>())
                {
                    ObjPicked.GetComponent<Rigidbody>().useGravity = false;
                    ObjPicked.GetComponent<Rigidbody>().isKinematic = true;
                    ObjPicked.GetComponent<Rigidbody>().detectCollisions = false;
                }
                else
                {
                    // המיקום והזווית של האוביקט ביחס לדמות הראשית לפני ההרמה כדי לדעת באיזו גובה וזווית להניחו
                    // זוכרים את הגובה ביחס לדמות הראשית, כי בזמן ההנחה, ישנן קרקעות בגבהים שונים
                    objPosition = transform.position - ObjTopick.transform.position;
                    objRotation = ObjTopick.transform.rotation;
                }

                ObjPicked.transform.SetParent(holdPoint); // הזזת האוביקט תחת מיקום האחיזה ביד הדמות הראשית
                ObjPicked.transform.localPosition = Vector3.zero; // איפוס מיקום האוביקט ביחס לנקודה האחיזה
                ObjPicked.transform.localRotation = Quaternion.identity; // איפוס זווית האוביקט ביחס לנקודת האחיזה
            }
        }
        else // אחרת הנחת האוביקט שהדמות הראשית מחזיקה
        {
            ObjPicked.transform.parent = null; // ביטול ההגדרה שהאוביקט תחת היד של הדמות הראשית

            // הפעלת הגדרות שבוטלו
            if (ObjPicked.GetComponent<NavMeshObstacle>())
                ObjPicked.GetComponent<NavMeshObstacle>().enabled = true;

            if (ObjPicked.GetComponent<Animator>())
                ObjPicked.GetComponent<Animator>().enabled = true;

            if (ObjPicked.GetComponent<Rigidbody>())
            {
                ObjPicked.GetComponent<Rigidbody>().useGravity = true;
                ObjPicked.GetComponent<Rigidbody>().isKinematic = false;
                ObjPicked.GetComponent<Rigidbody>().detectCollisions = true;
            }
            else
            {
                // (השבת האוביקט לגובה הקרקע (ביחס לגובה הדמות הראשית
                ObjPicked.transform.position = new Vector3(ObjPicked.transform.position.x, transform.position.y - objPosition.y, ObjPicked.transform.position.z);

                // השבת האוביקט לזווית המקורית לפני שהורם
                ObjPicked.transform.rotation = objRotation;
            }

            ObjPicked = null; // ביטול ההגדרה שהדמות הראשית מחזיקה אוביקט
        }
    }
}