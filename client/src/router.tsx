import { createBrowserRouter, Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from './stores/authStore';

// Lazy load pages
import { lazy, Suspense } from 'react';

const Layout = lazy(() => import('./components/Layout'));
const HomePage = lazy(() => import('./pages/HomePage'));
const AuthPage = lazy(() => import('./pages/AuthPage'));
const OnboardingPage = lazy(() => import('./pages/OnboardingPage'));
const MatchingResultsPage = lazy(() => import('./pages/MatchingResultsPage'));
const PsychologistsPage = lazy(() => import('./pages/PsychologistsPage'));
const PsychologistProfilePage = lazy(() => import('./pages/PsychologistProfilePage'));
const BookingPage = lazy(() => import('./pages/BookingPage'));
const ClientDashboardPage = lazy(() => import('./pages/ClientDashboardPage'));
const PsychologistDashboardPage = lazy(() => import('./pages/PsychologistDashboardPage'));
const AdminPage = lazy(() => import('./pages/AdminPage'));

function Loading() {
  return (
    <div className="flex items-center justify-center min-h-screen">
      <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
    </div>
  );
}

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuthStore();
  if (!isAuthenticated) {
    return <Navigate to="/auth" replace />;
  }
  return <>{children}</>;
}

function PsychologistRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isPsychologist } = useAuthStore();
  if (!isAuthenticated) {
    return <Navigate to="/auth" replace />;
  }
  if (!isPsychologist) {
    return <Navigate to="/" replace />;
  }
  return <>{children}</>;
}

function AdminRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isAdmin } = useAuthStore();
  if (!isAuthenticated) {
    return <Navigate to="/auth" replace />;
  }
  if (!isAdmin) {
    return <Navigate to="/" replace />;
  }
  return <>{children}</>;
}

export const router = createBrowserRouter([
  {
    path: '/',
    element: (
      <Suspense fallback={<Loading />}>
        <Layout>
          <Outlet />
        </Layout>
      </Suspense>
    ),
    children: [
      {
        index: true,
        element: (
          <Suspense fallback={<Loading />}>
            <HomePage />
          </Suspense>
        ),
      },
      {
        path: 'auth',
        element: (
          <Suspense fallback={<Loading />}>
            <AuthPage />
          </Suspense>
        ),
      },
      {
        path: 'onboarding',
        element: (
          <Suspense fallback={<Loading />}>
            <OnboardingPage />
          </Suspense>
        ),
      },
      {
        path: 'matching-results',
        element: (
          <Suspense fallback={<Loading />}>
            <MatchingResultsPage />
          </Suspense>
        ),
      },
      {
        path: 'psychologists',
        element: (
          <Suspense fallback={<Loading />}>
            <PsychologistsPage />
          </Suspense>
        ),
      },
      {
        path: 'psychologists/:id',
        element: (
          <Suspense fallback={<Loading />}>
            <PsychologistProfilePage />
          </Suspense>
        ),
      },
      {
        path: 'booking/:psychologistId',
        element: (
          <Suspense fallback={<Loading />}>
            <BookingPage />
          </Suspense>
        ),
      },
      {
        path: 'my-sessions',
        element: (
          <ProtectedRoute>
            <Suspense fallback={<Loading />}>
              <ClientDashboardPage />
            </Suspense>
          </ProtectedRoute>
        ),
      },
      {
        path: 'dashboard',
        element: (
          <PsychologistRoute>
            <Suspense fallback={<Loading />}>
              <PsychologistDashboardPage />
            </Suspense>
          </PsychologistRoute>
        ),
      },
      {
        path: 'admin',
        element: (
          <AdminRoute>
            <Suspense fallback={<Loading />}>
              <AdminPage />
            </Suspense>
          </AdminRoute>
        ),
      },
    ],
  },
]);
