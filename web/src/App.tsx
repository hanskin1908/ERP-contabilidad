import { Routes, Route, Navigate } from 'react-router-dom'
import Login from './pages/Login'
import Accounts from './pages/Accounts'
import Journal from './pages/Journal'
import ThirdParties from './pages/ThirdParties'
import Reports from './pages/Reports'
import Settings from './pages/Settings'
import Invoices from './pages/Invoices'
import InvoiceEditor from './pages/InvoiceEditor'
import JournalEditor from './pages/JournalEditor'
import { getToken } from './auth'
import Layout from './components/Layout'

function RequireAuth({ children }: { children: JSX.Element }) {
  if (!getToken()) return <Navigate to="/login" replace />
  return children
}

export default function App() {
  return (
    <Layout>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/accounts" element={<RequireAuth><Accounts /></RequireAuth>} />
        <Route path="/third-parties" element={<RequireAuth><ThirdParties /></RequireAuth>} />
        <Route path="/journal" element={<RequireAuth><Journal /></RequireAuth>} />
        <Route path="/journal/new" element={<RequireAuth><JournalEditor /></RequireAuth>} />
        <Route path="/journal/:id" element={<RequireAuth><JournalEditor /></RequireAuth>} />
        <Route path="/invoices" element={<RequireAuth><Invoices /></RequireAuth>} />
        <Route path="/invoices/new" element={<RequireAuth><InvoiceEditor /></RequireAuth>} />
        <Route path="/invoices/:id" element={<RequireAuth><InvoiceEditor /></RequireAuth>} />
        <Route path="/reports" element={<RequireAuth><Reports /></RequireAuth>} />
        <Route path="/settings" element={<RequireAuth><Settings /></RequireAuth>} />
        <Route path="/" element={<Navigate to="/accounts" />} />
      </Routes>
    </Layout>
  )
}
