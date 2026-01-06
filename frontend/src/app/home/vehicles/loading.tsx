import Spinner from "@/_components/Spinner";

export default function Loading() {
  return (
    <main className="lg:pl-62">
      <div className="flex items-center justify-center min-h-[50vh]">
        <Spinner />
      </div>
    </main>
  );
}
