// ═══════════════════════════════════════════════
//  CRM Pro — Shared API & Utility Layer
// ═══════════════════════════════════════════════

const API_BASE = 'http://localhost:5271/api';

// ── Auth helpers ──────────────────────────────
const Auth = {
  getToken: () => localStorage.getItem('token'),
  getRole:  () => localStorage.getItem('role'),
  getName:  () => localStorage.getItem('userName'),
  getId:    () => localStorage.getItem('userId'),
  isAdmin:  () => localStorage.getItem('role') === 'Admin',
  logout() {
    localStorage.clear();
    window.location.href = 'login.html';
  },
  require() {
    if (!this.getToken()) window.location.href = 'login.html';
  },
  requireAdmin() {
    if (!this.getToken()) { window.location.href = 'login.html'; return; }
    if (!this.isAdmin())  { window.location.href = 'sr-profile.html?id=' + this.getId(); }
  }
};

// Backwards compatibility aliases
function getToken()  { return Auth.getToken(); }
function logout()    { Auth.logout(); }
function requireAuth() { Auth.require(); }

// ── Fetch wrapper ─────────────────────────────
const Api = {
  _headers(extra = {}) {
    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${Auth.getToken()}`,
      ...extra
    };
  },
  async get(url) {
    const res = await fetch(API_BASE + url, { headers: this._headers() });
    if (res.status === 401) { Auth.logout(); return null; }
    if (!res.ok) throw new Error(`GET ${url} → ${res.status}`);
    return res.json();
  },
  async post(url, body) {
    const res = await fetch(API_BASE + url, {
      method: 'POST', headers: this._headers(), body: JSON.stringify(body)
    });
    if (res.status === 401) { Auth.logout(); return null; }
    if (!res.ok) throw new Error(await res.text());
    return res.json();
  },
  async postPublic(url, body) {
    const res = await fetch(API_BASE + url, {
      method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body)
    });
    if (!res.ok) throw new Error(await res.text());
    return res.json();
  },
  async put(url, body) {
    const res = await fetch(API_BASE + url, {
      method: 'PUT', headers: this._headers(),
      body: typeof body === 'string' ? body : JSON.stringify(body)
    });
    if (res.status === 401) { Auth.logout(); return null; }
    if (!res.ok) throw new Error(await res.text());
    return res.json();
  },
  async delete(url) {
    const res = await fetch(API_BASE + url, { method: 'DELETE', headers: this._headers() });
    if (res.status === 401) { Auth.logout(); return null; }
    if (!res.ok) throw new Error(await res.text());
    return res.json();
  },
  async download(url, filename) {
    const res = await fetch(API_BASE + url, { headers: this._headers() });
    if (!res.ok) throw new Error('Download failed');
    const blob = await res.blob();
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = filename;
    a.click();
    URL.revokeObjectURL(a.href);
  }
};