import { createFileRoute } from '@tanstack/react-router'
import {useEffect, useState} from "react";

export const Route = createFileRoute('/_authed/dashboard/')({
  component: DashboardPage,
})

function DashboardPage() {
  const [shortUrls, setShortUrls] = useState([])

  const getUrls = async () => {
    try {
      const res = await fetch('/api/shorturls', {
        method: 'GET',
        credentials: 'same-origin',
      })
      const data = await res.json()
      setShortUrls(data)
    } catch (error) {
      console.error(error)
    }
  }

  console.log(shortUrls)
  useEffect(() => {
    getUrls()
  },[])
  return (
      <h1>Dashboard</h1>
  )
}
