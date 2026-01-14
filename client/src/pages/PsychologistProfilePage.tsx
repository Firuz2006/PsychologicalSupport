import { useParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { usePsychologist } from '../api/hooks/usePsychologists';

export default function PsychologistProfilePage() {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  const { data: psychologist, isLoading } = usePsychologist(id!);

  if (isLoading) {
    return (
      <div className="flex justify-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (!psychologist) {
    return <div className="text-center py-12 text-gray-600">{t('common.error')}</div>;
  }

  return (
    <div className="max-w-3xl mx-auto">
      <div className="bg-white rounded-lg shadow-sm p-8">
        <div className="flex items-start space-x-6">
          {psychologist.photoPath ? (
            <img
              src={psychologist.photoPath}
              alt=""
              className="w-32 h-32 rounded-full object-cover"
            />
          ) : (
            <div className="w-32 h-32 rounded-full bg-blue-100 flex items-center justify-center text-blue-600 text-4xl font-semibold">
              {psychologist.firstName?.charAt(0) || 'P'}
            </div>
          )}

          <div className="flex-1">
            <div className="flex justify-between items-start">
              <div>
                <h1 className="text-2xl font-bold">
                  {psychologist.firstName} {psychologist.lastName}
                </h1>
                {psychologist.isVerified && (
                  <span className="inline-flex items-center text-green-600 text-sm mt-1">
                    ✓ {t('psychologist.verified')}
                  </span>
                )}
              </div>
              <div className="text-right">
                <div className="text-2xl font-bold text-blue-600">
                  {psychologist.pricePerSession} {t('psychologist.currency')}
                </div>
                <div className="text-sm text-gray-500">{t('psychologist.price')}</div>
              </div>
            </div>

            <div className="mt-4 grid grid-cols-2 gap-4 text-sm">
              <div>
                <span className="text-gray-500">{t('psychologist.experience')}:</span>
                <span className="ml-2 font-medium">
                  {psychologist.experienceYears} {t('psychologist.years')}
                </span>
              </div>
              <div>
                <span className="text-gray-500">{t('psychologist.languages')}:</span>
                <span className="ml-2 font-medium">{psychologist.languages.join(', ')}</span>
              </div>
              <div>
                <span className="text-gray-500">{t('psychologist.formats')}:</span>
                <span className="ml-2 font-medium">{psychologist.workFormats.join(', ')}</span>
              </div>
            </div>
          </div>
        </div>

        <div className="mt-6">
          <h2 className="font-semibold mb-2">{t('psychologist.specializations')}</h2>
          <div className="flex flex-wrap gap-2">
            {psychologist.specializations.map((spec) => (
              <span key={spec.id} className="px-3 py-1 bg-blue-50 text-blue-700 rounded-full text-sm">
                {spec.name}
              </span>
            ))}
          </div>
        </div>

        {psychologist.education && (
          <div className="mt-6">
            <h2 className="font-semibold mb-2">{t('psychologist.education')}</h2>
            <p className="text-gray-700">{psychologist.education}</p>
          </div>
        )}

        {psychologist.approachDescription && (
          <div className="mt-6">
            <h2 className="font-semibold mb-2">{t('psychologist.approach')}</h2>
            <p className="text-gray-700">{psychologist.approachDescription}</p>
          </div>
        )}

        <div className="mt-8">
          <Link
            to={`/booking/${psychologist.id}`}
            className="block w-full bg-blue-600 text-white text-center py-3 rounded-lg hover:bg-blue-700 font-medium"
          >
            {t('matching.bookSession')}
          </Link>
        </div>
      </div>
    </div>
  );
}
