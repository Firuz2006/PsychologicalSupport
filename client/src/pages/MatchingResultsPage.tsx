import { useLocation, Link, Navigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

interface PsychologistMatch {
  id: string;
  name: string;
  photoPath?: string;
  experienceYears: number;
  price: number;
  specializations: string[];
  matchReason: string;
  matchScore: number;
}

export default function MatchingResultsPage() {
  const { t } = useTranslation();
  const location = useLocation();
  const matches = location.state?.matches as PsychologistMatch[] | undefined;

  if (!matches) {
    return <Navigate to="/onboarding" replace />;
  }

  if (matches.length === 0) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-600 mb-4">{t('matching.noResults')}</p>
        <Link to="/psychologists" className="text-blue-600 hover:underline">
          {t('nav.psychologists')}
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-3xl mx-auto">
      <h1 className="text-2xl font-bold mb-6">{t('matching.results')}</h1>

      <div className="space-y-6">
        {matches.map((match, index) => (
          <div key={match.id} className="bg-white rounded-lg shadow-sm p-6">
            <div className="flex items-start space-x-4">
              <div className="relative">
                {match.photoPath ? (
                  <img
                    src={match.photoPath}
                    alt={match.name}
                    className="w-20 h-20 rounded-full object-cover"
                  />
                ) : (
                  <div className="w-20 h-20 rounded-full bg-blue-100 flex items-center justify-center text-blue-600 text-2xl font-semibold">
                    {match.name.charAt(0)}
                  </div>
                )}
                <div className="absolute -top-2 -right-2 w-8 h-8 bg-blue-600 text-white rounded-full flex items-center justify-center text-sm font-bold">
                  {index + 1}
                </div>
              </div>

              <div className="flex-1">
                <div className="flex justify-between items-start">
                  <div>
                    <h2 className="text-lg font-semibold">{match.name}</h2>
                    <p className="text-gray-600 text-sm">
                      {t('psychologist.experience')}: {match.experienceYears} {t('psychologist.years')}
                    </p>
                  </div>
                  <div className="text-right">
                    <div className="text-lg font-semibold text-blue-600">
                      {match.matchScore}%
                    </div>
                    <div className="text-sm text-gray-500">{t('matching.matchScore')}</div>
                  </div>
                </div>

                <div className="mt-3 flex flex-wrap gap-2">
                  {match.specializations.slice(0, 3).map((spec) => (
                    <span key={spec} className="px-2 py-1 bg-gray-100 text-gray-700 text-sm rounded">
                      {spec}
                    </span>
                  ))}
                </div>

                <div className="mt-3 p-3 bg-blue-50 rounded-lg">
                  <p className="text-sm text-gray-700">
                    <strong>{t('matching.matchReason')}:</strong> {match.matchReason}
                  </p>
                </div>

                <div className="mt-4 flex items-center justify-between">
                  <span className="text-lg font-semibold">
                    {match.price} {t('psychologist.currency')}
                  </span>
                  <div className="space-x-3">
                    <Link
                      to={`/psychologists/${match.id}`}
                      className="px-4 py-2 border border-blue-600 text-blue-600 rounded-lg hover:bg-blue-50"
                    >
                      {t('matching.viewProfile')}
                    </Link>
                    <Link
                      to={`/booking/${match.id}`}
                      className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                    >
                      {t('matching.bookSession')}
                    </Link>
                  </div>
                </div>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
