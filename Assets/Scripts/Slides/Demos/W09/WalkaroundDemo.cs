using System.Collections.Generic;
using UnityEngine;

/// <summary>Slide 10, the lab brief in one screen: three 3D sources (campfire, coin), the player's
/// own footsteps, and one 2D BGM track - all at once, with the ears walking around the mix.</summary>
public class WalkaroundDemo : AudioDemoBase
{
    [Header("Wired by the builder")]
    public Transform player;
    public AudioSource fireSource;
    public AudioSource coinSource;
    public AudioSource bgmSource;
    public AudioClip[] stepClips = new AudioClip[0];
    public Transform mixMeter;
    public TextMesh nearText;

    private const float StepInterval = 0.4f;
    private const float Speed = 3.5f;
    private const float CoinInterval = 2f;

    private float nextStep;
    private float nextCoin;
    private bool bgmOn = true;
    private int steps;

    private string nearName = "-";
    private float nearDistance;
    private float nearPan;
    private float nearLevel;
    private float mixLevel;

    public override string Title { get { return "Walkaround: Put It Together"; } }
    public override string Keys { get { return "ARROWS walk   B BGM on/off   R back to start"; } }

    public override void OnActivate()
    {
        bgmOn = true;
        steps = 0;
        nextStep = 0f;
        nextCoin = Time.time + CoinInterval;
        if (player != null) player.localPosition = Vector3.zero;
        FollowPlayer();

        if (source != null) source.Stop();
        if (fireSource != null) fireSource.Play();
        if (coinSource != null) coinSource.Play();
        if (bgmSource != null)
        {
            bgmSource.mute = false;
            bgmSource.Play();
        }
        ShowText();
    }

    public override void OnDeactivate()
    {
        if (source != null) source.Stop();
        if (fireSource != null) fireSource.Stop();
        if (coinSource != null) coinSource.Stop();
        if (bgmSource != null) bgmSource.Stop();
        PracticeMixer.Set(mixerGroup, PracticeMixer.BgmParam, 0f);
    }

    private void Update()
    {
        if (player == null) return;

        if (Input.GetKeyDown(KeyCode.B))
        {
            bgmOn = !bgmOn;
            if (bgmSource != null) bgmSource.mute = !bgmOn;
        }
        if (Input.GetKeyDown(KeyCode.R)) player.localPosition = Vector3.zero;

        Vector3 move = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0f);
        bool moving = move.sqrMagnitude > 0.01f;
        if (moving)
        {
            Vector3 p = player.localPosition + move.normalized * Speed * Time.deltaTime;
            p.x = Mathf.Clamp(p.x, -8.2f, 8.2f);
            p.y = Mathf.Clamp(p.y, -1f, 4.3f);
            player.localPosition = p;

            if (Time.time >= nextStep) Step();
        }

        if (Time.time >= nextCoin)
        {
            nextCoin = Time.time + CoinInterval;
            if (coinSource != null) coinSource.Play();
        }

        FollowPlayer();
        Measure();
        ShowText();
    }

    private void Step()
    {
        if (source == null || stepClips == null || stepClips.Length == 0) return;
        nextStep = Time.time + StepInterval;
        steps++;
        source.pitch = Random.Range(0.9f, 1.1f);
        source.PlayOneShot(stepClips[Random.Range(0, stepClips.Length)], 0.85f);
    }

    private void FollowPlayer()
    {
        if (listener == null || player == null) return;
        listener.transform.localPosition = new Vector3(player.localPosition.x, player.localPosition.y, 0f);
    }

    private void Measure()
    {
        float left, right;
        Ears(out left, out right);
        mixLevel = Mathf.Max(left, right);

        AudioSource nearest = null;
        float best = float.MaxValue;
        if (fireSource != null)
        {
            float d = Vector3.Distance(listener.transform.position, fireSource.transform.position);
            if (d < best) { best = d; nearest = fireSource; nearName = "campfire"; }
        }
        if (coinSource != null)
        {
            float d = Vector3.Distance(listener.transform.position, coinSource.transform.position);
            if (d < best) { best = d; nearest = coinSource; nearName = "coin"; }
        }

        nearDistance = nearest != null ? best : 0f;
        nearLevel = SourceLevel(nearest);
        nearPan = 0f;
        if (nearest != null && best > 0.001f)
        {
            Vector3 to = (nearest.transform.position - listener.transform.position) / best;
            nearPan = Mathf.Clamp(Vector3.Dot(to, listener.transform.right), -1f, 1f);
        }

        PracticeBars.Fill(mixMeter, mixLevel, 4.4f);
    }

    private void ShowText()
    {
        if (nearText != null)
            nearText.text = nearName + "   " + F(nearDistance) + " u   " + PanText(nearPan);
    }

    public override void Readout(List<string> lines)
    {
        lines.Add("nearest      " + nearName + "   " + F(nearDistance) + " u   " + PanText(nearPan));
        lines.Add("its output   " + Level(nearLevel) + " " + Db(nearLevel));
        lines.Add("what you hear " + Level(mixLevel) + " " + Db(mixLevel));
        lines.Add("bgm          " + (bgmOn ? "on" : "off") + "   (2D: same level wherever you stand)");
        lines.Add("the lab brief: 3D sources + footsteps + BGM + one mixer group");
    }
}
