using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationAsset
{
    static public UnityEngine.LocalizationAsset JaJpLocalizationAsset()
    {
        var localizationAsset = new UnityEngine.LocalizationAsset();

        localizationAsset.localeIsoCode = "ja-jp";
        localizationAsset.SetLocalizedString("AddStateMachineBehaviour�Ɏ��s���܂���", "AddStateMachineBehaviour�Ɏ��s���܂����B�����̏ꍇ�����͕ʂ̃c�[���ȂǂɃG���[���������Ă��邱�Ƃł��B�A�o�^�[�r���h�E�A�b�v���[�h�O��Console�E�B���h�E���m�F���ăG���[���������Ă���ēx��������������");
        localizationAsset.SetLocalizedString("���������Ԉ���Ă���p�����[�^������܂�", "���������Ԉ���Ă���p�����[�^������܂��B�u{0}�v�t�@�C���́u{1}�v�Ƃ������C���[�Ŏg�p����Ă���u{2}�v�Ƃ����p�����[�^�́AAnimatorController���ł́u{3}�v�Ƃ����^�ł����A�������Ɂu{4}�v���g���Ă��邽�ߏ����������������삵�܂���B���̂��߃M�~�b�N�⏈�������������삵�Ȃ��\��������܂��B�g�p���Ă���c�[����M�~�b�N�̑����̖�肾�Ǝv���邽�߁A���̃p�����[�^���g�p���Ă���c�[���̊J���҂ɘA�����Ă�������");
        localizationAsset.SetLocalizedString("Animator��Parameter�Ƃ��ēo�^����Ă��Ȃ��p�����[�^���������Ɏg���Ă��܂�", "Animator��Parameter�Ƃ��ēo�^����Ă��Ȃ��p�����[�^���������Ɏg���Ă��܂��B�u{0}�v�t�@�C���́u{1}�v�Ƃ������C���[�Ŏg�p����Ă���u{2}�v�Ƃ����p�����[�^���������Ɏg���Ă��܂����AAnimatorController��Parameters�ɂ̓p�����[�^������܂���B�g�p���Ă���c�[����M�~�b�N�̑����̖�肾�Ǝv���邽�߁A���̃p�����[�^���g�p���Ă���c�[���̊J���҂ɘA�����Ă�������");

        return localizationAsset;
    }

    static public List<UnityEngine.LocalizationAsset> GetList()
    {
        return new() { JaJpLocalizationAsset(), };
    }

    static public nadena.dev.ndmf.localization.Localizer ErrorLocalization()
    {
        return new nadena.dev.ndmf.localization.Localizer("ja-jp", () =>
        {
            return GetList();
        });
    }
}
