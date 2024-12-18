import React from "react";
import { motion } from "framer-motion";

const Spinner: React.FC = () => {
  return (
    <motion.div
      className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50 z-50"
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      transition={{ duration: 0.5 }}
    >
      <div className="spinner"></div>
    </motion.div>
  );
};

export default Spinner;
