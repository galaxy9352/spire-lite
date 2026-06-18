#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// 💡 [핵심] typeof 안에 Unit을 넣어줌으로써 이 에디터 코드는 Unit 컴포넌트 전용으로 격리됩니다.
// 뒤의 true는 Unit을 상속받은 자식 클래스(예: WarriorUnit, MageUnit)에도 이 설정을 공유하겠다는 의미입니다.
[CustomEditor(typeof(Unit), true)]
public class UnitEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 1. 공통 통합 헤더 배치
        EditorGUILayout.LabelField("Unit Status", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // 2. 부모와 자식에 분산되어 있던 변수명들을 각각 추출
        // (Unit 또는 자식 클래스 내부에 실재하는 변수명과 일치해야 합니다)
        SerializedProperty unitNameProp = serializedObject.FindProperty("unitName"); // 공통
        SerializedProperty maxHpProp = serializedObject.FindProperty("maxHP"); //공통
        SerializedProperty curHpProp = serializedObject.FindProperty("currentHP"); //공통
        SerializedProperty maxCostProp = serializedObject.FindProperty("maxCost"); //UnitPlayer만
        SerializedProperty curCostProp = serializedObject.FindProperty("curCost"); //UnitPlayer만, 하지만 UnitEditor에서 공통으로 다루면 알아서 처리됨

        // 3. 인스펙터 화면에 족보 격벽 없이 나란히 배치
        if (unitNameProp != null) EditorGUILayout.PropertyField(unitNameProp);
        if (maxHpProp != null) EditorGUILayout.PropertyField(maxHpProp);
        if (curHpProp != null) EditorGUILayout.PropertyField(curHpProp);
        if (maxCostProp != null) EditorGUILayout.PropertyField(maxCostProp);
        if (curCostProp != null) EditorGUILayout.PropertyField(curCostProp); EditorGUILayout.Space(15f);

        // 1. 공통 통합 헤더 배치
        EditorGUILayout.LabelField("Status Effects", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // 2. 부모와 자식에 분산되어 있던 변수명들을 각각 추출
        // (Unit 또는 자식 클래스 내부에 실재하는 변수명과 일치해야 합니다)
        SerializedProperty blockProp = serializedObject.FindProperty("block");
        SerializedProperty strProp = serializedObject.FindProperty("strength");
        SerializedProperty vulnerProp = serializedObject.FindProperty("vulnerable");
        SerializedProperty weakProp = serializedObject.FindProperty("weak");
        SerializedProperty poisonProp = serializedObject.FindProperty("poison");

        // 3. 인스펙터 화면에 족보 격벽 없이 나란히 배치
        if (blockProp != null) EditorGUILayout.PropertyField(blockProp);
        if (strProp != null) EditorGUILayout.PropertyField(strProp);
        if (vulnerProp != null) EditorGUILayout.PropertyField(vulnerProp);
        if (weakProp != null) EditorGUILayout.PropertyField(weakProp);
        if (poisonProp != null) EditorGUILayout.PropertyField(poisonProp); EditorGUILayout.Space(15f);

        // 1. 공통 통합 헤더 배치
        EditorGUILayout.LabelField("Head UI", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        // 2. 부모와 자식에 분산되어 있던 변수명들을 각각 추출
        // (Unit 또는 자식 클래스 내부에 실재하는 변수명과 일치해야 합니다)
        SerializedProperty blockTextProp = serializedObject.FindProperty("blockText");
        SerializedProperty damageTextProp = serializedObject.FindProperty("damageText");
        SerializedProperty strengthTextProp = serializedObject.FindProperty("strengthText");

        // 3. 인스펙터 화면에 족보 격벽 없이 나란히 배치
        if (blockTextProp != null) EditorGUILayout.PropertyField(blockTextProp);
        if (damageTextProp != null) EditorGUILayout.PropertyField(damageTextProp);
        if (strengthTextProp != null) EditorGUILayout.PropertyField(strengthTextProp); EditorGUILayout.Space(15f);


        SerializedProperty nextActionProp = serializedObject.FindProperty("nextAction");
        SerializedProperty nextActionValueProp = serializedObject.FindProperty("nextActionValue");
        if (nextActionProp != null)
        {
            EditorGUILayout.LabelField("Next Action (적 전용)", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(nextActionProp);
            if (nextActionValueProp != null) EditorGUILayout.PropertyField(nextActionValueProp); EditorGUILayout.Space(15f);

        }
        SerializedProperty damageColorProp = serializedObject.FindProperty("damageColor");
        SerializedProperty blockColorProp = serializedObject.FindProperty("blockColor");
        if(damageColorProp != null)
        {
            EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(damageColorProp);
            if (blockColorProp != null) EditorGUILayout.PropertyField(blockColorProp); EditorGUILayout.Space(15f);
        }

        SerializedProperty nextActionHitsProp = serializedObject.FindProperty("nextActionHits");
        if (nextActionHitsProp != null)
        {
            EditorGUILayout.LabelField("Unit02 전용 상태", EditorStyles.boldLabel);
            EditorGUILayout.Space(1);
            if (nextActionHitsProp != null) EditorGUILayout.PropertyField(nextActionHitsProp); EditorGUILayout.Space(15f);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif