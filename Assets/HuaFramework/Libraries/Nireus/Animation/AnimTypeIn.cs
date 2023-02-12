using UnityEngine;
using UnityEngine.UI;
using System.Collections;


namespace Nireus
{
    public class AnimTypeIn : MonoBehaviour
    {

        public string message = "";
        public float start_delta = 1f;
        public float type_delta = 0.01f;
        public AudioClip audio_clip;
        public Delegate.CallFuncVoid type_in_end_callback;

        private Text _text_comp;
        private AudioSource _audio_source;
        private int _next_line_index;
        private int _index;

        void Awake()
        {
            _text_comp = GetComponent<Text>();
            _audio_source = GetComponent<AudioSource>();
            if (_audio_source == null)
            {
                _audio_source = gameObject.AddComponent<AudioSource>();
            }
        }

        IEnumerator typeIn()
        {
            yield return new WaitForSeconds(start_delta);

            GameDebug.Log("message: " + message + " length: " + message.Length);

            _index = 1;
            _next_line_index = 0;

            while (_index <= message.Length)
            {
                _text_comp.text = message.Substring(0, _index++);
                if (_index >= _next_line_index) calueNextLineIndex();
                if (audio_clip != null) _audio_source.PlayOneShot(audio_clip);
                yield return new WaitForSeconds(type_delta);
            }

            _text_comp.text = message;
            if (type_in_end_callback != null) type_in_end_callback();
        }

        void calueNextLineIndex()
        {
            for (int i = _index + 1; i < message.Length; ++i)
            {
                if (message[i] == '\n')
                {
                    _next_line_index = i + 1;
                    return;
                }
            }
            _next_line_index = message.Length;
        }

        public void jump()
        {
            if (_index >= _next_line_index) return;
            _index = _next_line_index;
        }

        public void jumpToEnd()
        {
            _index = message.Length + 1;
        }

        public void start()
        {
            StartCoroutine("typeIn");
        }

        public void stop()
        {
            StopCoroutine("typeIn");
        }

        public void restart()
        {
            stop();
            start();
        }
    }
}
