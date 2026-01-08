import { Header } from "@/_components/layout/Header"
import { Hero } from "@/_components/layout/Hero"
import { PrimaryFeatures } from "@/_components/layout/PrimaryFeatures"
import { Pricing } from "@/_components/layout/Pricing"
import { Footer } from "@/_components/layout/Footer"

export default function Home() {
    return (
        <>
            <Header />
            <main>
                <Hero />
                <PrimaryFeatures />
                <Pricing />
            </main>
            <Footer />
        </>
    )
}
