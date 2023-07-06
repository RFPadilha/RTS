using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{
    public SkillData skill;//skill que será ativada
    GameObject _source;//objeto que possui skill
    Button _button;//botão que ativa skill
    bool _ready;//estado de prontidão da skill
    AudioSource _sourceContextualSource;

    public void Initialize(SkillData skill, GameObject source)
    {
        this.skill = skill;
        _source = source;
        // try to get the audio source from the source unit
        UnitManager um = source.GetComponent<UnitManager>();
        if (um != null)
            _sourceContextualSource = um.contextualSource;
    }
    //função que ativa skill
    public void Trigger(GameObject target = null)
    {
        if (!_ready) return;
        StartCoroutine(_WrappedTrigger(target));
    }
    //função que associa botão à skill
    public void SetButton(Button button)
    {
        _button = button;
        _SetReady(true);
    }
    //aplica efeitos da skill, bloqueando ativações sucessivas com cooldown
    private IEnumerator _WrappedTrigger(GameObject target)
    {
        if (_sourceContextualSource != null && skill.onStartSound)
            _sourceContextualSource.PlayOneShot(skill.onStartSound);
        yield return new WaitForSeconds(skill.castTime);
        if (_sourceContextualSource != null && skill.onEndSound)
            _sourceContextualSource.PlayOneShot(skill.onEndSound);
        skill.Trigger(_source, target);
        yield return new WaitForSeconds(skill.cooldown);
        _SetReady(true);
    }
    //define se skill pode ser usada ou não
    private void _SetReady(bool ready)
    {
        _ready = ready;
        if (_button != null) _button.interactable = ready;
    }
}