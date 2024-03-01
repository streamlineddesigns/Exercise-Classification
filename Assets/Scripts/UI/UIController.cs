using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Singleton;
        public ViewRegistry ViewRegistry;
        protected View currentView;
        protected View previousView;

        public void Awake()
        {
            if (Singleton == null) {
                Singleton = this;
            } else {
                Destroy(Singleton);
            }
        }

        public void Start()
        {
            currentView = ViewRegistry.getView(ViewName.ExerciseSelect);
            currentView.gameObject.SetActive(true);
        }

        public void Open(ViewName e)
        {
            View v = ViewRegistry.getView(e);
            ViewType vt = v.ViewType;

            if (vt == ViewType.Screen) {
                if (currentView != null) {
                    previousView = currentView;
                }  
                currentView = v;

                currentView.gameObject.SetActive(true);
                previousView.gameObject.SetActive(false);
            } else {
                v.gameObject.SetActive(true);
            }
        }

        public void OpenImmediately(ViewName e)
        {
            View v = ViewRegistry.getView(e);
            v.gameObject.SetActive(true);
        }

        public void CloseImmediately(ViewName e)
        {
            View v = ViewRegistry.getView(e);
            v.gameObject.SetActive(false);
        }

        public void Back()
        {
            ViewName e = previousView.ViewName;
            Open(e);
        }

        public void Home()
        {
            Open(ViewName.Start);
        }
}