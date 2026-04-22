import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { Toaster } from 'sonner';
import App from "./App.tsx";
import "./index.css";
import { getVersion } from '@/services/system.service';

const APP_VERSION_KEY = 'esm_app_version';

async function checkVersion() {
  try {
    const data = await getVersion();
    const currentVersion = data.version;
    const storedVersion = localStorage.getItem(APP_VERSION_KEY);

    if (storedVersion && storedVersion !== currentVersion) {
      localStorage.setItem(APP_VERSION_KEY, currentVersion);
      window.location.reload();
    } else {
      localStorage.setItem(APP_VERSION_KEY, currentVersion);
    }
  } catch {
    
  }
}

// 初始检测版本
checkVersion();

// 页面重新可见时检测版本（用户切回标签页时）
document.addEventListener('visibilitychange', () => {
  if (document.visibilityState === 'visible') {
    checkVersion();
  }
});

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <BrowserRouter>
      <App />
      <Toaster />
    </BrowserRouter>
  </StrictMode>
);
