import { useEffect, useState } from 'react'

const API_BASE_URL = 'http://localhost:5199'

function App() {
  const [apiStatus, setApiStatus] = useState<'checking' | 'ok' | 'unreachable'>('checking')

  useEffect(() => {
    fetch(`${API_BASE_URL}/`)
      .then((res) => setApiStatus(res.ok ? 'ok' : 'unreachable'))
      .catch(() => setApiStatus('unreachable'))
  }, [])

  return (
    <main>
      <h1>Employee Management</h1>
      <p>API status: {apiStatus}</p>
    </main>
  )
}

export default App
