using GB;
using UnityEngine;
using DG.Tweening;

public class TstMsgPop : UIScreen
{
    private Sequence tstSqc;
    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("TstMsgPop", this);
    }

    private void OnDisable()
    {
        Presenter.UnBind("TstMsgPop", this);

    }

    public void RegistButton()
    {
        foreach (var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key); });
    }

    public void OnButtonClick(string key)
    {
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "ShowTstMsg":
                ShowTstBox(LocalizationManager.GetValue(data.Get<string>()));
                break;
        }
    }
    private void ShowTstBox(string msg)
    {
        GameObject tstBox = mGameObject["Box"];
        CanvasGroup canvasGroup = tstBox.GetComponent<CanvasGroup>();

        if (tstSqc != null && tstSqc.IsActive())
            tstSqc.Kill();

        tstBox.SetActive(true);
        mTMPText["Ment"].text = msg;

        // 알파값 초기화
        canvasGroup.alpha = 0f;

        // Sequence로 모든 애니메이션을 한 번에 관리
        tstSqc = DOTween.Sequence()
            .SetUpdate(true) // 실제 시간 사용
            .Append(canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad)) // 페이드 인
            .AppendInterval(1.4f) // 대기 시간
            .Append(canvasGroup.DOFade(0f, 0.3f).SetEase(Ease.InQuad)) // 페이드 아웃
            .OnComplete(() =>
            {
                tstBox.SetActive(false);
                tstSqc = null;
                Close();
            });
    }
    public override void Refresh()
    {

    }
}