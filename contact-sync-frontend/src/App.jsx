import { useState, useEffect } from "react";
import axios from "axios";
import "./App.css";

const API = "http://localhost:5132/api/contacts";

const emptyForm = { firstName: "", lastName: "", email: "", phone: "", company: "" };

export default function App() {
  const [contacts, setContacts] = useState([]);
  const [filter, setFilter] = useState("All");
  const [showModal, setShowModal] = useState(false);
  const [editingContact, setEditingContact] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [syncing, setSyncing] = useState(null);
  const [toast, setToast] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => { fetchContacts(); }, []);

  async function fetchContacts() {
    try {
      const res = await axios.get(API);
      setContacts(res.data);
    } catch {
      showToast("Failed to load contacts", "error");
    } finally {
      setLoading(false);
    }
  }

  function showToast(message, type = "success") {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3000);
  }

  const filtered = filter === "All"
    ? contacts
    : contacts.filter(c => c.syncStatus === filter);

  const stats = {
    total: contacts.length,
    synced: contacts.filter(c => c.syncStatus === "Synced").length,
    pending: contacts.filter(c => c.syncStatus === "Pending").length,
    failed: contacts.filter(c => c.syncStatus === "Failed").length,
  };

  function openCreate() {
    setEditingContact(null);
    setForm(emptyForm);
    setShowModal(true);
  }

  function openEdit(contact) {
    setEditingContact(contact);
    setForm({
      firstName: contact.firstName,
      lastName: contact.lastName,
      email: contact.email,
      phone: contact.phone,
      company: contact.company,
    });
    setShowModal(true);
  }

  async function handleSubmit() {
    try {
      if (editingContact) {
        await axios.put(`${API}/${editingContact.id}`, form);
        showToast("Contact updated");
      } else {
        await axios.post(API, form);
        showToast("Contact created");
      }
      setShowModal(false);
      fetchContacts();
    } catch {
      showToast("Something went wrong", "error");
    }
  }

  async function handleDelete(id) {
    if (!confirm("Delete this contact?")) return;
    try {
      await axios.delete(`${API}/${id}`);
      showToast("Contact deleted");
      fetchContacts();
    } catch {
      showToast("Failed to delete", "error");
    }
  }

  async function handleSync(id) {
    setSyncing(id);
    try {
      const res = await axios.post(`${API}/${id}/sync`);
      showToast(res.data.message, res.data.success ? "success" : "error");
      fetchContacts();
    } catch {
      showToast("Sync failed", "error");
    } finally {
      setSyncing(null);
    }
  }

  async function handleSyncAll() {
    setSyncing("all");
    try {
      const res = await axios.post(`${API}/sync/pending`);
      showToast(`Synced ${res.data.succeeded} of ${res.data.totalProcessed} contacts`);
      fetchContacts();
    } catch {
      showToast("Batch sync failed", "error");
    } finally {
      setSyncing(null);
    }
  }

  return (
    <div className="app">
      <div className="header">
        <h1>ContactSync</h1>
        <p>Manage and sync contacts to your CRM in real time</p>
      </div>

      <div className="stats-bar">
        <div className="stat-card">
          <div className="number">{stats.total}</div>
          <div className="label">Total Contacts</div>
        </div>
        <div className="stat-card synced">
          <div className="number">{stats.synced}</div>
          <div className="label">Synced</div>
        </div>
        <div className="stat-card pending">
          <div className="number">{stats.pending}</div>
          <div className="label">Pending Sync</div>
        </div>
        <div className="stat-card failed">
          <div className="number">{stats.failed}</div>
          <div className="label">Failed</div>
        </div>
      </div>

      <div className="toolbar">
        <div className="toolbar-left">
          {["All", "Synced", "Pending", "Failed"].map(f => (
            <button
              key={f}
              className={`filter-btn ${filter === f ? "active" : ""}`}
              onClick={() => setFilter(f)}
            >
              {f}
            </button>
          ))}
        </div>
        <div style={{ display: "flex", gap: "10px" }}>
          <button className="btn btn-success" onClick={handleSyncAll} disabled={syncing === "all" || stats.pending === 0}>
            {syncing === "all" ? "Syncing..." : `Sync Pending (${stats.pending})`}
          </button>
          <button className="btn btn-primary" onClick={openCreate}>+ Add Contact</button>
        </div>
      </div>

      <div className="contacts-table">
        {loading ? (
          <div className="empty-state">Loading contacts...</div>
        ) : filtered.length === 0 ? (
          <div className="empty-state">No contacts found</div>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Phone</th>
                <th>Company</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map(contact => (
                <tr key={contact.id}>
                  <td><strong>{contact.firstName} {contact.lastName}</strong></td>
                  <td>{contact.email}</td>
                  <td>{contact.phone}</td>
                  <td>{contact.company}</td>
                  <td>
                    <span className={`badge badge-${contact.syncStatus.toLowerCase()}`}>
                      {contact.syncStatus}
                    </span>
                  </td>
                  <td style={{ display: "flex", gap: "8px" }}>
                    <button className="btn btn-sync" onClick={() => handleSync(contact.id)} disabled={syncing === contact.id}>
                      {syncing === contact.id ? "..." : "Sync"}
                    </button>
                    <button className="btn btn-sync" onClick={() => openEdit(contact)}>Edit</button>
                    <button className="btn btn-danger" onClick={() => handleDelete(contact.id)}>Delete</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h2>{editingContact ? "Edit Contact" : "Add Contact"}</h2>
            <div className="form-row">
              <div className="form-group">
                <label>First Name</label>
                <input value={form.firstName} onChange={e => setForm({ ...form, firstName: e.target.value })} placeholder="Sarah" />
              </div>
              <div className="form-group">
                <label>Last Name</label>
                <input value={form.lastName} onChange={e => setForm({ ...form, lastName: e.target.value })} placeholder="Chen" />
              </div>
            </div>
            <div className="form-group">
              <label>Email</label>
              <input value={form.email} onChange={e => setForm({ ...form, email: e.target.value })} placeholder="sarah@company.com" />
            </div>
            <div className="form-row">
              <div className="form-group">
                <label>Phone</label>
                <input value={form.phone} onChange={e => setForm({ ...form, phone: e.target.value })} placeholder="780-555-0100" />
              </div>
              <div className="form-group">
                <label>Company</label>
                <input value={form.company} onChange={e => setForm({ ...form, company: e.target.value })} placeholder="Acme Corp" />
              </div>
            </div>
            <div className="modal-actions">
              <button className="btn" style={{ background: "#f0f0f0" }} onClick={() => setShowModal(false)}>Cancel</button>
              <button className="btn btn-primary" onClick={handleSubmit}>
                {editingContact ? "Save Changes" : "Add Contact"}
              </button>
            </div>
          </div>
        </div>
      )}

      {toast && (
        <div className={`toast ${toast.type}`}>{toast.message}</div>
      )}
    </div>
  );
}